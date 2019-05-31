#include "Bot.h"
#include "BotInfo.h"

using namespace sc2;
using namespace std;

void BotInfo::update() {
    numDrones = bot.CountUnitType(UNIT_TYPEID::ZERG_DRONE);
    numEggDrones = bot.CountUnitEgg(ABILITY_ID::TRAIN_DRONE);
    numOver = bot.CountUnitType(UNIT_TYPEID::ZERG_OVERLORD);
    numEggOver = bot.CountUnitEgg(ABILITY_ID::TRAIN_OVERLORD);
    numEggQueen = bot.CountUnitEgg(ABILITY_ID::TRAIN_QUEEN, UNIT_TYPEID::ZERG_HATCHERY);

    cap = bot.cap;
    overTot = numOver + numEggOver;
    droneTot = numDrones + numEggDrones;

    queensTot = bot.CountUnitType(UNIT_TYPEID::ZERG_QUEEN) + numEggQueen;
    numZerglingsEggs = bot.CountUnitEgg(ABILITY_ID::TRAIN_ZERGLING);
    numRoachesEggs = bot.CountUnitEgg(ABILITY_ID::TRAIN_ROACH);
    numZerglings = bot.CountUnitType(UNIT_TYPEID::ZERG_ZERGLING);
    numRoaches = bot.CountUnitType(UNIT_TYPEID::ZERG_ROACH);
    zerglingTot = numZerglings + numZerglingsEggs;
    roachesTot = numRoaches + numRoachesEggs;
    bot.info.numHatchery = bot.CountUnitType(UNIT_TYPEID::ZERG_HATCHERY);
    totalUnits = queensTot + droneTot + zerglingTot + roachesTot;

}