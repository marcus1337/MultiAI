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

class FixBase(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot
        self.buildExtractor_cooldown = 10000
        self.buildExtractor_lastRun = 0

    async def debug(self):

        if(False):
            hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
            hatchery = hatcheries[0]

            larvas = self.bot.units(sc2.UnitTypeId.LARVA)
            print("krabba: ", larvas.closer_than(10.0, hatchery))

            
            if(larvas.exists):
                for larva in larvas:
                    print("distance")
                    print(larva.distance_to(hatchery.position))
                    
            drones = self.bot.units(sc2.UnitTypeId.DRONE)
            idleDrones = drones.idle
            #if(len(idleDrones) > 0):
            print(len(idleDrones) , " idle worker")
            return "running"
        return "fail"

    async def expandBase(self):
        if(self.bot.already_pending(sc2.UnitTypeId.HATCHERY)):
            return "fail"

        if(self.bot.units(sc2.UnitTypeId.HATCHERY).amount < 2):
            if(self.bot.minerals < 300):
                return "running"
            else:
                drones = self.bot.units(sc2.UnitTypeId.DRONE)
                chosenDrone = drones.random
                action = chosenDrone.build(sc2.UnitTypeId.HATCHERY, self.bot.info.expansion1)
                self.bot.commands.append(action)
                return "success"
        else:
            return "fail"
    

    
    async def injectHatchery(self):
        queens = self.bot.units(sc2.UnitTypeId.QUEEN)
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        #if(queens):
        action = None
        nReadyQueens = 0
        for queen in queens:
            abilities = await self.bot.get_available_abilities(queen)
            if sc2.AbilityId.EFFECT_INJECTLARVA in abilities:
                nReadyQueens += 1
                hatch = hatcheries[0]
                for hatchery in hatcheries:
                    if hatchery.position.distance_to(queen.position) < hatch.position.distance_to(queen.position):
                        hatch = hatchery
                action = queen(sc2.AbilityId.EFFECT_INJECTLARVA, hatch)

        if(action != None):
            self.bot.commands.append(action)
            if(nReadyQueens > 1):
                return "running"
            else:
                return "success"

        return "fail"

    async def buildQueen(self):
        queens = self.bot.units(sc2.UnitTypeId.QUEEN)
        nQueens = len(queens)
        queensOnCreate = self.bot.already_pending(sc2.UnitTypeId.QUEEN, True)
        nQueens += queensOnCreate
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        if(not self.bot.units(sc2.UnitTypeId.SPAWNINGPOOL).ready.exists):
            return "fail"
        if(queensOnCreate > 0):
            return "fail"
        if(len(hatcheries) > nQueens):
            queensToMake = len(hatcheries) - nQueens
            for hatchery in hatcheries:
                if(hatchery.is_ready):
                    queensForHatch = queens.closer_than(10.0, hatchery)
                    if(len(queensForHatch) < 1):
                        if(self.bot.minerals < 150):
                            return "running"
                        action = hatchery.train(sc2.UnitTypeId.QUEEN)
                        self.bot.commands.append(action)
                        return "success"
            return "fail"
        else:
            return "fail"
        return "fail"
        
    async def buildRoachWarren(self):
        if(not self.bot.units(sc2.UnitTypeId.SPAWNINGPOOL).ready.exists):
            return "fail"
        
        if(self.bot.units(sc2.UnitTypeId.ROACHWARREN).ready.exists or self.bot.already_pending(sc2.UnitTypeId.ROACHWARREN)):
            return "fail"


        if(self.bot.minerals < 150):
            return "running"
        
        if(self.bot.can_afford(sc2.UnitTypeId.ROACHWARREN)):
            hatchery = self.bot.units(sc2.UnitTypeId.HATCHERY).ready.first
            #print("kallepalle")
            #pos = hatchery.position.to2.towards(self.game_info.map_center, 7)
            pos = hatchery.position.towards(self.bot.game_info.map_center, 7)
            pos = await self.bot.find_placement(building=sc2.UnitTypeId.ROACHWARREN, near=pos, max_distance=7)
            drones = self.bot.units(sc2.UnitTypeId.DRONE)
            chosenDrone = drones.random
            action = chosenDrone.build(sc2.UnitTypeId.ROACHWARREN, pos)
            self.bot.commands.append(action)
        else:
            return "fail"

        return "success"

    async def mutateToLair(self):
        print("kalle 1")
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        hatchery = hatcheries[0]
        #hatchery = self.bot.units(sc2.UnitTypeId.HATCHERY).ready.first
        print("kalle 2")
        if self.bot.units(sc2.UnitTypeId.SPAWNINGPOOL).ready.exists:
            print("kalle 3")
            if not self.bot.units(sc2.UnitTypeId.LAIR).exists and hatchery.noqueue:
                print("kalle 4")
                if self.bot.can_afford(sc2.UnitTypeId.LAIR):
                    print("kalle 5")
                    action = hatchery.build(sc2.UnitTypeId.LAIR)
                    print("kalle 6")
                    self.bot.commands.append(action)
                    print("kalle 7")
                    return "success"
                else:
                    print("kalle 8")
                    return "running"
                    #await self.do(hq.build(LAIR)
        print("kalle 9")
        return "fail"

        
    async def buildSpawningpool(self):
        if(self.bot.units(sc2.UnitTypeId.SPAWNINGPOOL).ready.exists or self.bot.already_pending(sc2.UnitTypeId.SPAWNINGPOOL)):
            return "fail"

        if(self.bot.minerals < 200):
            return "running"
        
        if(self.bot.can_afford(sc2.UnitTypeId.SPAWNINGPOOL)):
            hatchery = self.bot.units(sc2.UnitTypeId.HATCHERY).ready.first
            #print("kallepalle")
            #pos = hatchery.position.to2.towards(self.game_info.map_center, 7)
            pos = hatchery.position.towards(self.bot.game_info.map_center, 7)
            pos = await self.bot.find_placement(building=sc2.UnitTypeId.SPAWNINGPOOL, near=pos, max_distance=7)
            drones = self.bot.units(sc2.UnitTypeId.DRONE)
            chosenDrone = drones.random
            action = chosenDrone.build(sc2.UnitTypeId.SPAWNINGPOOL, pos)
            self.bot.commands.append(action)
        else:
            return "fail"

        return "success"

    async def setWorkersOnExtractor(self):
        extractors = self.bot.units(sc2.UnitTypeId.EXTRACTOR)
        for extractor in extractors:
            if(extractor.assigned_harvesters < extractor.ideal_harvesters):
                hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
                hatch = hatcheries[0]
                for hatchery in hatcheries:
                    if hatchery.position.distance_to(extractor.position) < hatch.position.distance_to(extractor.position):
                        hatch = hatchery
                drones = self.bot.units(sc2.UnitTypeId.DRONE)
                chosenDrone = drones.closer_than(10.0, hatchery).random
                action = chosenDrone.gather(extractor)
                self.bot.commands.append(action)
                return "success"
        return "fail"


    async def buildExtractor(self):
        if(int(round(time.time() * 1000)) - self.buildExtractor_lastRun < self.buildExtractor_cooldown):
            return "fail"

        extractors = self.bot.units(sc2.UnitTypeId.EXTRACTOR)
        drones = self.bot.units(sc2.UnitTypeId.DRONE)
        if(len(drones) < 15):
            return "fail"

        if(len(drones) < 20 and extractors.amount == 1):
            return "fail"
        
        if(len(drones) < 25 and extractors.amount == 2):
            return "fail"
        



        
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        for hatchery in hatcheries:
            if(hatchery.is_ready):
                
                drones = self.bot.units(sc2.UnitTypeId.DRONE)
                hatcheryDrones = drones.closer_than(10.0, hatchery)

                if(len(hatcheryDrones) < 10):
                    return "fail"

                baseExtractors = extractors.closer_than(10.0, hatchery)
                if(len(baseExtractors) < 2):
                    if(self.bot.minerals < 25):
                        return "running"    
                    chosenDrone = drones.closest_to(hatchery)
                    #pos = self.bot.state.vespene_geyser.closest_to(chosenDrone.position)
                    print("kalle 1")
                    geysers = self.bot.state.vespene_geyser.closer_than(10.0, hatchery)
                    print("kalle 2")
                    print("leeen: " , len(geysers))
                    for geyser in geysers:
                        print("kalle 3")
                        if(await self.bot.can_place(sc2.UnitTypeId.EXTRACTOR, geyser.position)):
                            print("kalle 4")
                            action = chosenDrone.build(sc2.UnitTypeId.EXTRACTOR, geyser)
                            print("kalle 5")
                            self.bot.commands.append(action)
                            self.buildExtractor_lastRun = int(round(time.time() * 1000))
                            return "success"

                    return "fail"

                        #pos = await self.bot.find_placement(building=sc2.UnitTypeId.EXTRACTOR, near=hatchery.position, max_distance=10)
                        #print(self.bot.state.vespene_geyser)

                        #action = chosenDrone.build(sc2.UnitTypeId.EXTRACTOR, pos)
                        #self.bot.commands.append(action)
                        #return "success"

        return "fail"
    
    async def handleIdleWorkers(self):
        drones = self.bot.units(sc2.UnitTypeId.DRONE)
        idleDrones = drones.idle
        if(len(idleDrones) > 0):
            action = None
            hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
            for hatchery in hatcheries:
                if(hatchery.assigned_harvesters < hatchery.ideal_harvesters):
                    closest_mineral_patch = self.bot.state.mineral_field.closest_to(hatchery)
                    action = idleDrones.closest_to(hatchery).gather(closest_mineral_patch)
            if(action != None):
                self.bot.commands.append(action)
                if(len(idleDrones) > 1):
                    return "running"
                else:
                    return "success"
            else:
                closest_mineral_patch = self.bot.state.mineral_field.closest_to(hatcheries[0])
                action = idleDrones.closest_to(hatcheries[0]).gather(closest_mineral_patch)
                #action = idleDrones.closest_to(hatcheries[0]).gather()
                self.bot.commands.append(action)
                if(len(idleDrones) > 1):
                    return "running"
                else:
                    return "success"
        else:
            return "fail"

    async def exportWorkersToOtherBase(self):        
        drones = self.bot.units(sc2.UnitTypeId.DRONE)
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        workerToMove = None
        workersToMove = 0
        for hatchery in hatcheries:
            if(hatchery.assigned_harvesters > hatchery.ideal_harvesters):
                #for drone in 
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
        


    async def buildOverlord(self):
        makeOverlord = False
        overlordsOnCreate = self.bot.already_pending(sc2.UnitTypeId.OVERLORD)

        if self.bot.supply_left < 2 and overlordsOnCreate == 0:
            makeOverlord = True

        if(self.bot.supply_left < 5 and self.bot.minerals > 300 and overlordsOnCreate < 3):
            makeOverlord = True
            
        if(makeOverlord):
            larvas = self.bot.units(sc2.UnitTypeId.LARVA)
            if((not larvas.exists) or (not self.bot.can_afford(sc2.UnitTypeId.OVERLORD))):
                return "running"

            action = larvas.random.train(sc2.UnitTypeId.OVERLORD)
            self.bot.commands.append(action)
            return "success"

        return "fail"

    async def buildDrone(self):
        cap = self.bot.supply_cap
        supply = self.bot.supply_used
        
        if(supply == cap):
            return "running"

        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        dronesOnCreate = self.bot.already_pending(sc2.UnitTypeId.DRONE)
        
        wantsDrone = False
        for hatchery in hatcheries:
            if(hatchery.assigned_harvesters + dronesOnCreate < hatchery.ideal_harvesters):
                wantsDrone = True
                if(self.bot.minerals < 50):
                    continue

                larvas = self.bot.units(sc2.UnitTypeId.LARVA)
                if(len(larvas.closer_than(10.0, hatchery)) == 0):
                    noLarvas = True
                    continue  
                action = larvas.closer_than(10.0, hatchery).random.train(sc2.UnitTypeId.DRONE)
                self.bot.commands.append(action)
                return "success"
        if(wantsDrone):
            dronesMining = dronesOnCreate
            dronesWantedOnMining = 0
            for hatchery in hatcheries:
                dronesMining += hatchery.assigned_harvesters
                dronesWantedOnMining += hatchery.ideal_harvesters

            if(dronesMining < dronesWantedOnMining):
                if(self.bot.minerals < 50):
                    return "running"

                larvas = self.bot.units(sc2.UnitTypeId.LARVA)
                if larvas.exists:
                    action = larvas.random.train(sc2.UnitTypeId.DRONE)
                    self.bot.commands.append(action)
                    return "success"
                else:
                    return "running"
            else:
                return "fail"
        else:
            return "fail"
        return "fail"
