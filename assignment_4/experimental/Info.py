import sc2

class Info(object):

    numDrones = 0
    numDroneEggs = 0
    numDronesAll = 0
    numOverlords = 0
    numOverlordEggs = 0
    numOverlordsAll = 0
    numQueens = 0
    numQueensMaking = 0
    numQueensAll = 0
    numUnitsAll = 0
    numZerglings = 0
    numZerglingEggs = 0
    numZerglingsAll = 0
    numRoaches = 0
    numRoachesEggs = 0
    numRoachesAll = 0
    hasSpawnPool = False

    numHatcheries = 0
    numHatcheriesBuilt = 1
    hatcheries = None
    initsDone = False
    expansion1 = None

    def __init__(self, my_bot):
        self.bot : sc2.BotAI = my_bot

    def update(self):
        self.numDrones = len(self.bot.units(sc2.UnitTypeId.DRONE))
        self.numDroneEggs = self.bot.already_pending(sc2.UnitTypeId.DRONE)
        self.numDronesAll = self.numDrones + self.numDroneEggs

        self.numOverlords = len(self.bot.units(sc2.UnitTypeId.OVERLORD))
        self.numOverlordEggs = self.bot.already_pending(sc2.UnitTypeId.OVERLORD)
        self.numOverlordsAll = self.numOverlords + self.numOverlordEggs

        self.numQueens = len(self.bot.units(sc2.UnitTypeId.QUEEN))
        self.numQueensMaking = self.bot.already_pending(sc2.UnitTypeId.QUEEN, True)
        self.numQueensAll = self.numQueens + self.numQueensMaking

        self.numZerglings = len(self.bot.units(sc2.UnitTypeId.ZERGLING))
        self.numZerglingEggs = self.bot.already_pending(sc2.UnitTypeId.ZERGLING)
        self.numZerglingsAll = self.numZerglings + self.numZerglingEggs

        self.numRoaches = len(self.bot.units(sc2.UnitTypeId.ROACH))
        self.numRoachesEggs = self.bot.already_pending(sc2.UnitTypeId.ROACH)
        self.numRoachesAll = self.numRoaches + self.numRoachesEggs
        if(len(self.bot.units(sc2.UnitTypeId.SPAWNINGPOOL)) != 0):
            self.hasSpawnPool = True

        self.numHatcheries = len(self.bot.units(sc2.UnitTypeId.HATCHERY))
        self.hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
