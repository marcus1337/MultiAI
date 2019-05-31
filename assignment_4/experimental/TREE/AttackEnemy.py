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

class AttackEnemy(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot

    async def buildRoach(self):
        #if(self.bot.minerals < 75 or self.bot.vespene < 25):
        #    return "running"
        if(not self.bot.units(sc2.UnitTypeId.ROACHWARREN).ready.exists):
            return "fail"

        if(self.bot.can_afford(sc2.UnitTypeId.ROACH)):
            larvas = self.bot.units(sc2.UnitTypeId.LARVA)
            if(larvas.exists):
                #larvas.random.build(sc2.UnitTypeId.ROACH)
                action = larvas.random.train(sc2.UnitTypeId.ROACH)
                self.bot.commands.append(action)
                return "success"
        
        return "fail"
        

        if(self.bot.can_afford(sc2.UnitTypeId.ROACH)):
            larvas = self.bot.units(sc2.UnitTypeId.LARVA)
            if(larvas.exists):
                #larvas.random.build(sc2.UnitTypeId.ROACH)
                action = larvas.random.train(sc2.UnitTypeId.ROACH)
                self.bot.commands.append(action)
                return "success"
        
        return "fail"

    async def attackEnemyBase(self):
        roaches = self.bot.units(sc2.UnitTypeId.ROACH)
        if(roaches.amount > 20):
            for roach in roaches:
                action = roach.attack(self.bot.enemy_start_locations[0])
                self.bot.commands.append(action)
            return "running"
        return "fail"
        
        
