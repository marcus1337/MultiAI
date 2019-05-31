import sc2
import math

class BaseCheck(object):
    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot

    async def update(self):
        if(self.bot.info.numDronesAll >= 30 and self.bot.info.hasSpawnPool and self.bot.info.numHatcheriesBuilt >= 2):
            return "success"
        return "fail"