import sc2
import math
import random
from sc2.ids.upgrade_id import UpgradeId

class AttackHandler(object):
    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot
        self.evochamberStarted = False
        self.warrenStarted = False
        self.missile1Started = False
        self.glialreconStarted = False
        self.forces = []
        self.isattacking = False
        self.retry1 = -1
        self.retry2 = -1
        self.allbases = None
        self.numscouts = 0

    async def enemiesDetectedAtBase(self):
        enemies = self.bot.known_enemy_units
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        if(len(hatcheries) == 0):
            return "fail"
        self.baseEnemies = []

        for e in enemies:
            if e.position.distance2_to(hatcheries[0].position) < 50:
                self.baseEnemies.append(e)

        if len(self.baseEnemies) > 0:
            return "fail"

        return "success"

    async def attackBaseEnemies(self):
        forces = self.bot.units(sc2.UnitTypeId.ROACH) | self.bot.units(sc2.UnitTypeId.ZERGLING) | self.bot.units(sc2.UnitTypeId.HYDRALISK) | self.bot.units(sc2.UnitTypeId.QUEEN)
        for unit in forces:
            re = random.choice(self.baseEnemies)
            await self.bot.do(unit.attack(re))
        return "success"

    async def stopgroup(self):

        return "success"

    async def regroup(self):
        if self.isattacking:
            return "success"

        pos = self.select_target()
        forces = self.bot.units(sc2.UnitTypeId.ROACH) | self.bot.units(sc2.UnitTypeId.ZERGLING) | self.bot.units(sc2.UnitTypeId.HYDRALISK)
        if len(forces) == 0:
            return "success"
        best = forces[0]
        mindist = best.position.distance2_to(pos)
        for unit in forces:
            tmpdist = unit.position.distance2_to(pos)
            if tmpdist < mindist:
                mindist = tmpdist
                best = unit

        anymove = False
        for unit in forces:
            distleft = unit.position.distance2_to(pos)
            if unit.is_idle and distleft > 15:
                await self.bot.do(unit.move(best))
                anymove = True

        if anymove:
            return "running"
        return "success"

    async def makeSqouts(self):
        zerglings = self.bot.units(sc2.UnitTypeId.ZERGLING)
        zergEgges = self.bot.already_pending(sc2.UnitTypeId.ZERGLING, True)

        if (len(zerglings) + zergEgges) >= 2:
            return "success"
        larvae = self.bot.units(sc2.UnitTypeId.LARVA)
        if len(larvae) == 0:
            return "fail"

        if self.bot.can_afford(sc2.UnitTypeId.ZERGLING):
            await self.bot.do(larvae.random.train(sc2.UnitTypeId.ZERGLING))
            return "success"

        return "fail"

    async def makeOverlord(self):
        larvae = self.bot.units(sc2.UnitTypeId.LARVA)
        if self.bot.info.numOverlordEggs >= 4:
            return "success"

        if self.bot.supply_left < 8 and self.bot.supply_cap < 200:
            if self.bot.can_afford(sc2.UnitTypeId.OVERLORD) and larvae.exists:
                await self.bot.do(larvae.random.train(sc2.UnitTypeId.OVERLORD))
                return "success"
            else:
                return "fail"
        return "success"

    async def makeZergling(self):
        larvae = self.bot.units(sc2.UnitTypeId.LARVA)
        if self.bot.supply_left >= 1:
            if self.bot.can_afford(sc2.UnitTypeId.ZERGLING) and larvae.exists and self.bot.vespene < 50:
                await self.bot.do(larvae.random.train(sc2.UnitTypeId.ZERGLING))
        return "success"

    async def makeRoach(self):
        larvae = self.bot.units(sc2.UnitTypeId.LARVA)
        if self.bot.supply_left >= 1:
            if self.bot.can_afford(sc2.UnitTypeId.ROACH) and larvae.exists:
                await self.bot.do(larvae.random.train(sc2.UnitTypeId.ROACH))
        return "success"

    async def makeLair(self):
        lairsmaking = self.bot.already_pending(sc2.UnitTypeId.LAIR, True)
        if(lairsmaking > 0):
            return "success"
        numlairs = len(self.bot.units(sc2.UnitTypeId.LAIR))
        if numlairs > 0:
            return "success"

        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)
        if self.bot.can_afford(sc2.UnitTypeId.LAIR):
            err = await self.bot.do(hatcheries[0](sc2.AbilityId.UPGRADETOLAIR_LAIR))
            if not err:
                return "success"
        return "fail"

    async def makeEvoChamber(self):
        numEvos = len(self.bot.units(sc2.UnitTypeId.EVOLUTIONCHAMBER))
        if numEvos > 0:
            return "success"
        self.retry1 -= 1
        if self.evochamberStarted and self.retry1 > 0:
            return "fail"
        if not self.bot.can_afford(sc2.UnitTypeId.EVOLUTIONCHAMBER):
            return "fail"
        hq = self.bot.townhalls.first
        err = await self.bot.build(sc2.UnitTypeId.EVOLUTIONCHAMBER, near=hq)
        if err:
            return "fail"
        self.evochamberStarted = True
        self.retry1 = 15
        return "success"

    async def makeRoachWarren(self):
        numWarrens = len(self.bot.units(sc2.UnitTypeId.ROACHWARREN))
        if numWarrens > 0:
            return "success"
        self.retry2 -= 1
        if self.warrenStarted and self.retry2 > 0:
            return "running"
        if not self.bot.can_afford(sc2.UnitTypeId.ROACHWARREN):
            return "fail"
        hq = self.bot.townhalls.first
        err = await self.bot.build(sc2.UnitTypeId.ROACHWARREN, near=hq)
        if err:
            return "fail"
        self.retry2 = 15

        self.warrenStarted = True
        return "success"

    async def researchMissiles(self):
        numEvos = len(self.bot.units(sc2.UnitTypeId.EVOLUTIONCHAMBER))
        evos = self.bot.units(sc2.UnitTypeId.EVOLUTIONCHAMBER)
        if numEvos == 0:
            return "fail"
        if not self.bot.info.evochamberDone:
            return "fail"
        if self.missile1Started:
            return "success"
        missresearch = self.bot.already_pending_upgrade(UpgradeId.ZERGMISSILEWEAPONSLEVEL1)
        if missresearch > 0 and missresearch < 1:
            return "running"
        if missresearch == 1:
            return "success"
        if not self.bot.can_afford(sc2.AbilityId.RESEARCH_ZERGMISSILEWEAPONSLEVEL1):
            return "fail"
        await self.bot.do(evos[0](sc2.AbilityId.RESEARCH_ZERGMISSILEWEAPONSLEVEL1))
        self.missile1Started = True
        return "success"

    async def researchGlialRecon(self):
        numWarren = len(self.bot.units(sc2.UnitTypeId.ROACHWARREN))
        warrens = self.bot.units(sc2.UnitTypeId.ROACHWARREN)
        if numWarren == 0:
            return "fail"
        if not self.bot.info.roachwarrenDone:
            return "fail"
        if self.glialreconStarted:
            return "success"
        glialresearch = self.bot.already_pending_upgrade(UpgradeId.GLIALRECONSTITUTION)
        if glialresearch > 0 and glialresearch < 1:
            return "fail"
        if glialresearch == 1:
            return "success"
        if not self.bot.can_afford(sc2.AbilityId.RESEARCH_GLIALREGENERATION):
            return "fail"
        await self.bot.do(warrens[0](sc2.AbilityId.RESEARCH_GLIALREGENERATION))
        self.glialreconStarted = True
        return "success"



    async def scoutEnemy(self):
        forces = self.bot.units(sc2.UnitTypeId.ROACH) | self.bot.units(sc2.UnitTypeId.ZERGLING) | self.bot.units(sc2.UnitTypeId.HYDRALISK)

        allunits = self.bot.expansion_locations.values()

        if self.allbases == None:
            self.allbases = []
            for a in allunits:
                if len(a) > 0:
                    self.allbases.append(a[0])
        n = len(self.allbases)
        if n == 0:
            return "success"

        for unit in forces:
            if unit.is_idle:
                randnum = random.randint(0, n-1)
                dist_target = self.allbases[randnum].position.distance2_to(unit.position)
                if(dist_target > 14):
                    await self.bot.do(unit.move(self.allbases[randnum].position))
                del self.allbases[randnum]
                n -= 1
                if n == 0:
                    return "success"
        return "success"

    async def know_enemy_pos(self):
        if self.bot.known_enemy_structures.exists or self.bot.known_enemy_units.exists:
            return "success"
        return "failure"

    def select_target(self):
        if self.bot.known_enemy_structures.exists:
            return random.choice(self.bot.known_enemy_structures).position
        if self.bot.known_enemy_units.exists:
            return random.choice(self.bot.known_enemy_units).position
        return self.bot.enemy_start_locations[0]


    #async def moveToEnemy(self):
    #    if len(self.forces) > 0:
    #        print()

    async def attack(self):
        forces = self.bot.units(sc2.UnitTypeId.ROACH) | self.bot.units(sc2.UnitTypeId.ZERGLING) | self.bot.units(sc2.UnitTypeId.HYDRALISK)
        if len(forces) > 20:
            target = self.select_target()
            for unit in forces:
                if unit.is_idle:
                    await self.bot.do(unit.attack(target))
            return "success"
        return "fail"

    async def update(self):
        return "success"