import sc2


class DroneHandler(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot

    def update(self):
        larvas = self.bot.units(sc2.UnitTypeId.LARVA)
        if len(larvas) == 0:
            return "running"

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

        return "success"