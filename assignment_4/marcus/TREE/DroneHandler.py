import sc2


class DroneHandler(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot
        self.lastQ = 1

    def exportWorkersToOtherBase(self):
        drones = self.bot.units(sc2.UnitTypeId.DRONE)
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        workerToMove = None
        workersToMove = 0
        for hatchery in hatcheries:
            if(hatchery.assigned_harvesters > hatchery.ideal_harvesters):
                workerToMove = drones.closest_to(hatchery)
                workersToMove = hatchery.assigned_harvesters - hatchery.assigned_harvesters
                break

        hatchNeedingWorkers = None
        for hatchery in hatcheries:
            if(hatchery.assigned_harvesters < hatchery.ideal_harvesters):
                hatchNeedingWorkers = hatchery
                break

        if(workerToMove != None and hatchNeedingWorkers != None):
            mineralsInOtherBase = self.bot.state.mineral_field.closest_to(hatchNeedingWorkers)
            action = workerToMove.gather(mineralsInOtherBase)
            self.bot.commands.append(action)
            if(workersToMove > 1):
                return "running"
            else:
                return "success"
        else:
            return "fail"

    async def makeOverlord(self):
        larvae = self.bot.units(sc2.UnitTypeId.LARVA)
        if self.bot.info.numOverlordEggs >= 2:
            return "success"

        if self.bot.supply_left < 3 and self.bot.supply_cap < 200:
            if self.bot.can_afford(sc2.UnitTypeId.OVERLORD) and larvae.exists:
                await self.bot.do(larvae.random.train(sc2.UnitTypeId.OVERLORD))
                return "success"
            else:
                return "fail"
        return "success"

    async def makeQueen(self):
        hatches = self.bot.info.numHatcheriesBuilt
        queens = self.bot.info.numQueensAll
        hq = self.bot.townhalls.first

        if hatches >= 2 and self.bot.minerals > 300 and queens < 4:
            action = hq.train(sc2.UnitTypeId.QUEEN)
            self.bot.commands.append(action)
            return "success"

        return "success"

    async def update(self):
        larvas = self.bot.units(sc2.UnitTypeId.LARVA)
        if len(larvas) == 0:
            return "running"

        self.exportWorkersToOtherBase()

        larva = larvas[0]
        cap = self.bot.supply_cap
        supply = self.bot.supply_used
        overlords = self.bot.info.numOverlordsAll
        drones = self.bot.info.numDronesAll
        pool = self.bot.info.hasSpawnPool
        queens = self.bot.info.numQueensAll
        hatches = self.bot.info.numHatcheriesBuilt
        hatcheries = self.bot.info.hatcheries

        if overlords < 2:
            action = larva.train(sc2.UnitTypeId.OVERLORD)
            self.bot.commands.append(action)
            return "success"

        if pool and drones >= 15 and queens == 0:
            action = hatcheries[0].train(sc2.UnitTypeId.QUEEN)
            self.bot.commands.append(action)
            return "success"

        if drones < 17:
            action = larva.train(sc2.UnitTypeId.DRONE)
            self.bot.commands.append(action)
            return "success"

        if drones >= 17 and overlords == 2:
            action = larva.train(sc2.UnitTypeId.OVERLORD)
            self.bot.commands.append(action)
            return "success"

        if hatches >= 2 and queens == 1 and self.bot.info.numQueens == 1:
            action = self.bot.info.hatcheries[1].train(sc2.UnitTypeId.QUEEN)
            self.bot.commands.append(action)
            return "success"

        if drones < 24 and self.bot.info.numQueens == 1:
            action = larva.train(sc2.UnitTypeId.DRONE)
            self.bot.commands.append(action)
            return "success"

        if drones >= 24 and overlords == 3:
            action = larva.train(sc2.UnitTypeId.OVERLORD)
            self.bot.commands.append(action)
            return "success"

        if drones <= 39 and overlords == 4:
            action = larva.train(sc2.UnitTypeId.DRONE)
            self.bot.commands.append(action)
            return "success"


        return "success"