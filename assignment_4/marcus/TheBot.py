import sc2
from Info import Info
from TREE.behaviorTree import Strategies

class TheBot(sc2.BotAI):

    steps = 0
    commands = []

    def on_start(self):
        print("Starting strategy...")
        self.info = Info(self)
        self.tree = Strategies(self).getTree()
        self.gameend = False

        for a in self.enemy_start_locations:
            self.info.all_locations.append(a)
        #temp = list(map(lambda x : x['title'], songs))


        #print(test)

    async def on_building_construction_complete(self, unit):
        if(self.steps == 0):
            return
        if unit.type_id == sc2.UnitTypeId.HATCHERY:
            self.info.numHatcheriesBuilt += 1
            closest_mineral_patch = self.state.mineral_field.closest_to(unit)
            action = unit(sc2.AbilityId.RALLY_HATCHERY_WORKERS, closest_mineral_patch)
            self.commands.append(action)
        if unit.type_id == sc2.UnitTypeId.EVOLUTIONCHAMBER:
            self.info.evochamberDone = True
        if unit.type_id == sc2.UnitTypeId.ROACHWARREN:
            self.info.roachwarrenDone = True

    async def on_unit_destroyed(self, unit_tag):
        if(self.steps == 0):
            return
        if unit_tag == sc2.UnitTypeId.HATCHERY:
            self.info.numHatcheriesBuilt -= 1


    async def on_building_construction_started(self, unit):
        if(self.steps == 0):
            return
        if unit.type_id == sc2.UnitTypeId.HATCHERY:
            closest_mineral_patch = self.state.mineral_field.closest_to(unit)
            action = unit(sc2.AbilityId.RALLY_HATCHERY_WORKERS, closest_mineral_patch)
            self.commands.append(action)
        if unit.type_id == sc2.UnitTypeId.EVOLUTIONCHAMBER:
            self.info.evochamberStart = True
        if unit.type_id == sc2.UnitTypeId.ROACHWARREN:
            self.info.roachwarrenStart = True

    async def on_unit_created(self, unit):
        if(self.steps == 0):
            return
        if unit.type_id == sc2.UnitTypeId.DRONE:
            closest_mineral_patch = self.state.mineral_field.closest_to(unit)
            action = unit(sc2.AbilityId.HARVEST_GATHER_DRONE, closest_mineral_patch)
            await self.do(action)

    async def on_step(self, iteration):
        if self.gameend:
            return

        if not self.info.initsDone:
            self.info.initsDone = True
            self.info.expansion1 = await self.get_next_expansion()

        self.steps = iteration
        self.info.update()
        await self.tree.run()
        self.tree.printTree(printDescription=True, colorForBuild=False)
        self.tree.clean(full=False)

        await self.do_actions(self.commands)
        self.commands = []

    def on_end(self, result):
        self.gameend = True

