import sc2
import math

#from sc2.constants import *
#from sc2.player import Bot, Computer
import time
import random

#import sc2
#from sc2 import Race, Difficulty
#from sc2.constants import *
#from sc2.player import Bot, Computer

class DefendBase(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot
        self.moveArmyToRamp_cooldown = 6000
        self.moveArmyToRamp_lastRun = 0

    async def moveArmyToRamp(self):
        if(int(round(time.time() * 1000)) - self.moveArmyToRamp_lastRun < self.moveArmyToRamp_cooldown):
            return "fail"

        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        #pos = 
        #for hatchery in hatcheries:

        roaches = self.bot.units(sc2.UnitTypeId.ROACH)
        for roach in roaches:
            action = roach.move(self.bot.main_base_ramp.barracks_correct_placement)
            self.bot.commands.append(action)
        return "fail"

        
