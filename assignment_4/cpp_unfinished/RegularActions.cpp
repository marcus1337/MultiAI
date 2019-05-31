#include "Actions.h"
#include "Bot.h"
#include <iostream>

using namespace sc2;
using namespace std;

WorkerHandler::Status WorkerHandler::update()
{

    /*Units dronesIdle = bot.getAnyUnits(UNIT_TYPEID::ZERG_DRONE, true);
    int numDrones = bot.info.numDrones;
    int numEggDrones = bot.info.numEggDrones;
    int numOver = bot.info.numOver;
    int numEggOver = bot.info.numEggOver;
    int cap = bot.info.cap;
    int overTot = numOver + numEggOver;
    int droneTot = numDrones + numEggDrones;*/
    Units queens = bot.getAnyUnits(UNIT_TYPEID::ZERG_QUEEN);
    for (auto& q : queens) {
        const Unit* closeHatch = bot.FindNearestHatchery(q->pos);
        if (q->energy >= 25 && q->orders.empty()) {
            bot.Actions()->UnitCommand(q, ABILITY_ID::EFFECT_INJECTLARVA, closeHatch);
        }
    }



    return Node::Status::Success;
}