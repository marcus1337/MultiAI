#include "Actions.h"

#include "Bot.h"
#include <iostream>

using namespace sc2;
using namespace std;

DroneSpawn::Status DroneSpawn::update()
{
    Units hatches = bot.getAnyUnits(UNIT_TYPEID::ZERG_HATCHERY);
    if (hatches.empty())
        return Node::Status::Failure;

    const Unit* larva = bot.getLarva();
    if (larva == nullptr || bot.Observation()->GetMinerals() < 50)
        return Node::Status::Running;

   

    Units dronesIdle = bot.getAnyUnits(UNIT_TYPEID::ZERG_DRONE, true);
    int numDrones = bot.info.numDrones;
    int numEggDrones = bot.info.numEggDrones;
    int numOver = bot.info.numOver;
    int numEggOver = bot.info.numEggOver;
    int cap = bot.info.cap;
    int overTot = numOver + numEggOver;
    int droneTot = numDrones + numEggDrones;

  //  cout << "BEFORE " << bot.steps << " " << bot.info.hasSpawnPool << " Queens " << bot.info.queensTot << " "
  //      << droneTot << " " << bot.info.totalUnits << " OVER " << overTot << endl;

    if (cap == 14 && overTot < 2) {
        bot.Actions()->UnitCommand(larva, ABILITY_ID::TRAIN_OVERLORD);

        return Node::Status::Success;
    }
    if (overTot >= 2 && droneTot <= 14) {
        bot.Actions()->UnitCommand(larva, ABILITY_ID::TRAIN_DRONE);
        return Node::Status::Success;
    }

    if (bot.info.hasSpawnPool && droneTot >= 15 && bot.info.queensTot == 0) {
        bot.Actions()->UnitCommand(hatches[0], ABILITY_ID::TRAIN_QUEEN);
        return Node::Status::Success;
    }

    if (bot.info.totalUnits < 17) {
        bot.Actions()->UnitCommand(larva, ABILITY_ID::TRAIN_DRONE);
        return Node::Status::Success;
    }
    if (bot.info.totalUnits >= 17 && overTot == 2) {
        bot.Actions()->UnitCommand(larva, ABILITY_ID::TRAIN_OVERLORD);
        return Node::Status::Success;
    }

    if (bot.info.droneTot >= 17 && overTot == 3 && bot.info.zerglingTot < 1) {
        bot.Actions()->UnitCommand(larva, ABILITY_ID::TRAIN_ZERGLING);
        return Node::Status::Success;
    }

    if (hatches.size() >= 2 && bot.info.numQueens == 1 && bot.info.queensTot == 1) {
        bot.Actions()->UnitCommand(hatches[1], ABILITY_ID::TRAIN_QUEEN);
        return Node::Status::Success;
    }

    if (bot.info.numQueens == 1 && bot.info.totalUnits < 24) {
        bot.Actions()->UnitCommand(larva, ABILITY_ID::TRAIN_DRONE);
        return Node::Status::Success;
    }

    if (bot.info.totalUnits >= 24 && overTot == 3) {
        bot.Actions()->UnitCommand(larva, ABILITY_ID::TRAIN_OVERLORD);
        return Node::Status::Success;
    }

    return Node::Status::Success;
}

BuildNode::Status BuildNode::update() {
    // cout << "AFTER " << bot.steps << endl;
    building = false;
    const Unit* drone = bot.getDrone();
    Units bases = bot.getAnyUnits(UNIT_TYPEID::ZERG_HATCHERY);

    if (bases.empty())
        return Node::Status::Failure;

    if (drone == nullptr)
        return Node::Status::Failure;

    bool buildOK = true;
    bool running = false;

    if (bot.info.droneTot >= 14 && !bot.info.hasSpawnPool) {
        running = true;
        buildOK = tryBuildStructure(drone, bases[0], ABILITY_ID::BUILD_SPAWNINGPOOL, UNIT_TYPEID::ZERG_DRONE);
    }

    if (bot.info.numHatchery == 1 && bot.info.droneTot >= 15) {
        running = true;
        if (bot.Observation()->GetMinerals() < 60) {
            return Node::Status::Running;
        }

        if (bot.Observation()->GetMinerals() < 300 && !startedExpanding) {
            startedExpanding = true;
            expTag = drone->tag;
            Point3D closest_expansion = getClosestExpansion(drone, ABILITY_ID::BUILD_HATCHERY);
            closest_expansion.x += 0.5;
            closest_expansion.y += 0.5;
            bot.Actions()->UnitCommand(drone, ABILITY_ID::MOVE, closest_expansion);
        }
        else {
            const Unit* drone2 = bot.getDrone(expTag);
            buildOK = TryExpand(drone2, ABILITY_ID::BUILD_HATCHERY, UNIT_TYPEID::ZERG_DRONE);
        }
    }

    if (!running || building)
        return Node::Status::Success;
    if (running)
        return Node::Status::Running;
    return Node::Status::Failure;
}

bool BuildNode::tryBuildStructure(const Unit* drone, const Unit* baseHatch, ABILITY_ID ability_type_for_structure, UNIT_TYPEID unit_type = UNIT_TYPEID::ZERG_DRONE) {

    const ObservationInterface* observation = bot.Observation();

    Units units = observation->GetUnits(Unit::Alliance::Self);
    for (const auto& unit : units) {
        for (const auto& order : unit->orders) {
            if (order.ability_id == ability_type_for_structure) {
                building = true;
                return false;
            }
        }
    }

    float rx = GetRandomScalar();
    float ry = GetRandomScalar();
    auto location = Point2D(baseHatch->pos.x + rx * 15.0f, baseHatch->pos.y + ry * 15.0f);

    if (!bot.Query()->Placement(ability_type_for_structure, location)) {
        return false;
    }

    bot.Actions()->UnitCommand(drone, ability_type_for_structure, location);

    return true;
}

Point3D BuildNode::getClosestExpansion(const Unit* drone, ABILITY_ID build_ability) {
    float minimum_distance = std::numeric_limits<float>::max();
    Point3D closest_expansion;
    for (const auto& expansion : bot.expansions_) {
        float current_distance = Distance2D(bot.startLocation_, expansion);
        if (current_distance < .01f) {
            continue;
        }

        if (current_distance < minimum_distance) {
            if (bot.Query()->Placement(build_ability, expansion) && bot.Query()->PathingDistance(drone, expansion) > 0.1f) {
                closest_expansion = expansion;
                minimum_distance = current_distance;
            }
        }
    }
    return closest_expansion;
}

bool BuildNode::TryExpand(const Unit* drone , ABILITY_ID build_ability, UnitTypeID worker_type) {
    const ObservationInterface* observation = bot.Observation();
  
    Point3D closest_expansion = getClosestExpansion(drone, build_ability);

    Units units = observation->GetUnits(Unit::Alliance::Self);
    for (const auto& unit : units) {
        for (const auto& order : unit->orders) {
            if (order.ability_id == build_ability) {
                building = true;
                return false;
            }
        }
    }

    bot.Actions()->UnitCommand(drone, build_ability, closest_expansion);
    return true;
}
