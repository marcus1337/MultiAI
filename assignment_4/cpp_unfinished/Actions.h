#pragma once
#include "BrainTree.h"
#include <sc2api/sc2_api.h>

class Bot;
using namespace sc2;

class DroneSpawn : public BrainTree::Node
{
    Bot& bot;


public:

    DroneSpawn(Bot& _bot) : bot(_bot) {
    }

    Status update() override;
};

class BuildNode : public BrainTree::Node
{
    Bot& bot;
    uint64_t expTag = 0;
public:

    BuildNode(Bot& _bot) : bot(_bot) {
    }
    bool building = false;
    bool startedExpanding = false;

    bool tryBuildStructure(const sc2::Unit* drone, const sc2::Unit* baseHatch, sc2::ABILITY_ID ability_type_for_structure, sc2::UNIT_TYPEID unit_type);
    bool TryExpand(const sc2::Unit* drone, ABILITY_ID build_ability, UnitTypeID worker_type);
    Point3D getClosestExpansion(const Unit* drone, ABILITY_ID build_ability);

    Status update() override;
};

class WorkerHandler : public BrainTree::Node {
    Bot& bot;

public:

    WorkerHandler(Bot& _bot) : bot(_bot) {
    }
    Status update() override;

};