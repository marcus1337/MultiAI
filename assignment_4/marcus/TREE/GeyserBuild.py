import sc2
import math

class GeyserBuild(object):
    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot

    async def update(self):
        if(self.bot.info.numDronesAll < 22):
            return "success"

        if len(self.bot.units(sc2.UnitTypeId.EXTRACTOR)) < 2 and not self.bot.already_pending(sc2.UnitTypeId.EXTRACTOR):
            if self.bot.can_afford(sc2.UnitTypeId.EXTRACTOR):
                drone = self.bot.workers.random
                target = self.bot.state.vespene_geyser.closest_to(drone.position)
                self.bot.commands.append(drone.build(sc2.UnitTypeId.EXTRACTOR, target))

        if self.bot.info.numDronesAll > 30 and len(self.bot.units(sc2.UnitTypeId.EXTRACTOR)) < 4 and not self.bot.already_pending(sc2.UnitTypeId.EXTRACTOR):
            if self.bot.can_afford(sc2.UnitTypeId.EXTRACTOR):
                drone = self.bot.workers.random
                target = self.bot.state.vespene_geyser.closest_to(drone.position)
                self.bot.commands.append(drone.build(sc2.UnitTypeId.EXTRACTOR, target))

        return "success"

class GeyserWorkers(object):
    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot

    async def update(self):
        for a in self.bot.units(sc2.UnitTypeId.EXTRACTOR):
            if a.assigned_harvesters < a.ideal_harvesters:
                w = self.bot.workers.closer_than(20, a)
                if w.exists:
                    self.bot.commands.append(w.random.gather(a))

        return "success"