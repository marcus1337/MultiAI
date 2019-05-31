import sc2
import math

class BuildHandler(object):

    expTag = 0
    building = False
    startedExpanding = False
    spawning_pool_started = False

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot

    def update(self):
        self.building = False
        buildOK = True
        running = False

        numHatches = self.bot.info.numHatcheries
        hatcheries = self.bot.info.hatcheries
        drones = self.bot.units(sc2.UnitTypeId.DRONE)
        pool = self.bot.info.hasSpawnPool
        bot = self.bot

        if self.bot.info.numDronesAll >= 14 and not pool:
            running = True
            pos = hatcheries[0].position.towards(bot.game_info.map_center, 7)
            if not (self.bot.units(sc2.UnitTypeId.SPAWNINGPOOL).exists or self.bot.already_pending(sc2.UnitTypeId.SPAWNINGPOOL)):
                action = drones[0].build(sc2.UnitTypeId.SPAWNINGPOOL, pos)
                self.bot.commands.append(action)

        if numHatches == 1 and self.bot.info.numDronesAll >= 15 and pool:
            running = True
            if self.bot.minerals < 110:
                return "running"

            if self.bot.minerals < 300 and not self.startedExpanding:
                self.startedExpanding = True
                self.expTag = drones[0].tag
                action = drones[0].move(self.bot.info.expansion1)
                self.bot.commands.append(action)
            else:
                drone = None
                for dr in drones:
                    if self.expTag == dr.tag:
                        drone = dr
                action = drone.build(sc2.UnitTypeId.HATCHERY, self.bot.info.expansion1)
                self.bot.commands.append(action)


        if not running or self.building:
            return "success"
        return "running"
