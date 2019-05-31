#pragma once

#include <sc2api/sc2_api.h>
#include "sc2lib/sc2_lib.h"
#include <iostream>
#include "BrainTree.h"
#include "Actions.h"
#include "BotInfo.h"

using namespace sc2;
using namespace std;

typedef BrainTree::Node::Ptr Ptr;



class StrategyHandler {
    Bot& bot;
public:
    StrategyHandler(Bot& _bot) : bot(_bot) {

    }

    Ptr simpleStrategy() {
        Ptr tree = BrainTree::Builder()
            .composite<BrainTree::Sequence>()
            .leaf<WorkerHandler>(std::ref(bot))
            .leaf<BuildNode>(std::ref(bot))
            .leaf<DroneSpawn>(std::ref(bot))
            .end()
            .build();
        return tree;
    }

};

class Bot : public Agent {
public:
    typedef std::vector<const Unit*> Units;
    BotInfo info;

    std::vector<Point3D> expansions_;
    Point3D startLocation_;
    Point3D staging_location_;

    Units getAnyUnits(UNIT_TYPEID type, bool onlyIdle = false) {
        Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(type));
        Filter filter;
        if (onlyIdle) {
            Units unitsIdle;
            for (const Unit* u : units) {
                if (u->orders.empty())
                    unitsIdle.push_back(u);
            }
            return unitsIdle;
        }
        return units;
    }

    const Unit* getDrone(uint64_t id = 0) {

        const Unit* unit = nullptr;
        Units drones = getAnyUnits(UNIT_TYPEID::ZERG_DRONE);
        if (!drones.empty()) {
            unit = drones[0];
        }
        for (auto& d : drones) {
            if (d->orders.empty()) {
                unit = d;
            }
            if (id != 0 && d->tag == id) {
                unit = d;
                break;
            }
        }
        return unit;
    }

    const Unit* getLarva() {
        const Unit* unit = nullptr;
        Units larvas = getAnyUnits(UNIT_TYPEID::ZERG_LARVA);
        if (!larvas.empty()) {
            unit = larvas[0];
        }

        return unit;
    }

    const Unit* getLarvaFromHatch(int index) {
        const Unit* unit = nullptr;
        Units larvas = getAnyUnits(UNIT_TYPEID::ZERG_LARVA);
        Units hatches = getAnyUnits(UNIT_TYPEID::ZERG_HATCHERY);
        if (hatches.size() - 1 < index)
            return nullptr;
        const Unit* hatch = hatches[index];
        for (auto& larv : larvas) {
            float d = DistanceSquared2D(larv->pos, hatch->pos);
            if (d < 3.f) {
                return larv;
            }
        }
        return nullptr;
    }

    size_t cap = 14;
    size_t numUnits = 0;
    StrategyHandler strategy;
    Ptr tree;
    int64_t steps = 0;

    size_t CountUnitType(UNIT_TYPEID unit_type) {
        return Observation()->GetUnits(Unit::Alliance::Self, IsUnit(unit_type)).size();
    }

    size_t CountUnitEgg(ABILITY_ID abilityId, UNIT_TYPEID hustyp = UNIT_TYPEID::ZERG_EGG) {
        size_t res = 0;
        Units eggs = getAnyUnits(hustyp);
        for (auto& u : eggs) {
            for (auto& o : u->orders) {
                if (o.ability_id == abilityId) {
                    res++;
                }
            }
        }
        return res;
    }

    size_t countUnits() {
        size_t totUnits = CountUnitType(UNIT_TYPEID::ZERG_DRONE);
        totUnits += CountUnitType(UNIT_TYPEID::ZERG_BANELING);
        totUnits += CountUnitType(UNIT_TYPEID::ZERG_QUEEN);
        totUnits += CountUnitType(UNIT_TYPEID::ZERG_ROACH);
        return totUnits;
    }

    Bot() : strategy(*this), info(*this) {

    }

    virtual void OnGameStart() final {
        std::cout << "Starting strategy ..." << std::endl;
        tree = strategy.simpleStrategy();

        expansions_ = search::CalculateExpansionLocations(Observation(), Query());
        startLocation_ = Observation()->GetStartLocation();
        staging_location_ = startLocation_;
    }

    virtual void OnStep() final {
        steps = Observation()->GetGameLoop();
        if (steps == 0)
            return;
        info.update();
        tree->update();

    }

    virtual void OnUnitCreated(const Unit* unit) final {
        numUnits = countUnits();
        if (steps == 0)
            return;
        if (!unit->Self) {

            return;
        }

        if (unit->Self && unit->unit_type == UNIT_TYPEID::ZERG_HATCHERY) {
            cap += 6;
           
        }
        if (unit->Self && unit->unit_type == UNIT_TYPEID::ZERG_OVERLORD) {
            cap += 8;
        }

        if (unit->unit_type == UNIT_TYPEID::ZERG_SPAWNINGPOOL) {
            info.hasSpawnPool = true;
        }

        if (unit->unit_type == UNIT_TYPEID::ZERG_QUEEN) {
            info.numQueens++;
        }

        if (unit->unit_type == UNIT_TYPEID::ZERG_HATCHERY) {
            const Unit* mine = FindNearestMineralPatch(Point2D(unit->pos));
            Actions()->UnitCommand(unit, ABILITY_ID::RALLY_HATCHERY_WORKERS, mine);
        }


    }

    virtual void OnUnitDestroyed(const Unit* unit) final {
        numUnits = countUnits();
        if (unit->Self && unit->unit_type == UNIT_TYPEID::ZERG_HATCHERY) {
            cap -= 6;
        }
        if (unit->Self && unit->unit_type == UNIT_TYPEID::ZERG_OVERLORD) {
            cap -= 8;
        }
    }

    virtual void OnGameStop() final {
        Control()->SaveReplay("LastSave.SC2Replay"); //don't work atm
    }

    virtual void OnUnitIdle(const Unit* unit) final {
        if (steps == 0)
            return;
    }


    const Unit* FindNearestHatchery(const Point2D& start) {
        Units units = getAnyUnits(UNIT_TYPEID::ZERG_HATCHERY);
        float distance = std::numeric_limits<float>::max();
        const Unit* target = nullptr;
        for (const auto& u : units) {
            float d = DistanceSquared2D(u->pos, start);
            if (d < distance) {
                distance = d;
                target = u;
            }
        }
        return target;
    }

    const Unit* FindNearestMineralPatch(const Point2D& start) {
        Units units = Observation()->GetUnits(Unit::Alliance::Neutral);
        float distance = std::numeric_limits<float>::max();
        const Unit* target = nullptr;
        for (const auto& u : units) {
            if (u->unit_type == UNIT_TYPEID::NEUTRAL_MINERALFIELD) {
                float d = DistanceSquared2D(u->pos, start);
                if (d < distance) {
                    distance = d;
                    target = u;
                }
            }
        }
        return target;
    }

};
