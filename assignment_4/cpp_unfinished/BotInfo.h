#pragma once


class Bot;

struct BotInfo {
    Bot& bot;
public:

    int numDrones;
    int numEggDrones;
    int numOver;
    int numEggOver;
    int cap;
    int overTot;
    int droneTot;
    int numHatchery = 1;
    int numQueens = 0;
    int queensTot;
    int numEggQueen;
    int totalUnits = 0;
    int numZerglings = 0;
    int numRoaches = 0;
    int numZerglingsEggs = 0;
    int numRoachesEggs = 0;
    int zerglingTot;
    int roachesTot;

    bool hasSpawnPool = false;

    BotInfo(Bot& _bot) : bot(_bot) {
    }

    void update();

};