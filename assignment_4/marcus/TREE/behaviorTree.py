import sc2
from termcolor import colored
from TREE import DroneHandler, BuildHandler, WorkerHandler, GeyserBuild, BaseCheck, AttackHandler
from TREE.GeyserBuild import GeyserWorkers

class Strategies(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot
        self.droneHandler = DroneHandler.DroneHandler(self.bot)
        self.buildHandler = BuildHandler.BuildHandler(self.bot)
        self.workerHandler = WorkerHandler.WorkerHandler(self.bot)
        self.geyserBuild = GeyserBuild.GeyserBuild(self.bot)
        self.geyserWorkers = GeyserWorkers(self.bot)
        self.baseCheck = BaseCheck.BaseCheck(self.bot)
        self.attackHandler = AttackHandler.AttackHandler(self.bot)

    def getTree(self):
        tree = Tree(Node(parent=None, nodeType="root", id="0"))
        Node.tree = tree

        Node(parent=tree.getNode("0"), nodeType="sequence")

        nodDef = Node(parent=tree.getNode("00"), nodeType="selector")
        Node(parent=nodDef, nodeType="leaf", description="scout needed", action=self.attackHandler.enemiesDetectedAtBase)
        Node(parent=nodDef, nodeType="leaf", description="scout enemy", action=self.attackHandler.attackBaseEnemies)

        Node(parent=tree.getNode("00"), nodeType="leaf", description="manage misc units", action=self.workerHandler.update)
        Node(parent=tree.getNode("00"), nodeType="leaf", description="expand base", action=self.buildHandler.update)
        Node(parent=tree.getNode("00"), nodeType="leaf", description="build misc units", action=self.droneHandler.update)
        Node(parent=tree.getNode("00"), nodeType="leaf", description="make overlords", action=self.droneHandler.makeOverlord)
        Node(parent=tree.getNode("00"), nodeType="leaf", description="expand geyser", action=self.geyserBuild.update)
        Node(parent=tree.getNode("00"), nodeType="leaf", description="geyser workers", action=self.geyserWorkers.update)
        Node(parent=tree.getNode("00"), nodeType="leaf", description="defensive queens", action=self.droneHandler.makeQueen)
        #Node(parent=tree.getNode("00"), nodeType="leaf", description="check progress", action=self.baseCheck.update)

        Node(parent=tree.getNode("00"), nodeType="leaf", description="make sqouts", action=self.attackHandler.makeSqouts)

        nodAtk = Node(parent=tree.getNode("00"), nodeType="sequence")

        nodScout = Node(parent=nodAtk, nodeType="selector")
        Node(parent=nodScout, nodeType="leaf", description="scout needed", action=self.attackHandler.know_enemy_pos)
        Node(parent=nodScout, nodeType="leaf", description="scout enemy", action=self.attackHandler.scoutEnemy)

        nodPrep = Node(parent=nodAtk, nodeType="sequence")
        Node(parent=nodPrep, nodeType="leaf", description="build lair", action=self.attackHandler.makeLair)
        Node(parent=nodPrep, nodeType="leaf", description="build evo chamber", action=self.attackHandler.makeEvoChamber)
        Node(parent=nodPrep, nodeType="leaf", description="research missiles", action=self.attackHandler.researchMissiles)
        Node(parent=nodPrep, nodeType="leaf", description="build warren", action=self.attackHandler.makeRoachWarren)
        Node(parent=nodPrep, nodeType="leaf", description="research speed", action=self.attackHandler.researchGlialRecon)

        Node(parent=nodAtk, nodeType="leaf", description="make overlord", action=self.attackHandler.makeOverlord)
        Node(parent=nodAtk, nodeType="leaf", description="make roach", action=self.attackHandler.makeRoach)
        Node(parent=nodAtk, nodeType="leaf", description="make zergling", action=self.attackHandler.makeZergling)

        #Node(parent=nodAtk, nodeType="leaf", description="build attack", action=self.attackHandler.regroup)


        Node(parent=nodAtk, nodeType="leaf", description="attack", action=self.attackHandler.attack)

        return tree


class Tree:
    root = None

    def __init__(self, root):
        self.root = root

    async def run(self):
        await Tree.runHelper(self.root)

    async def runHelper(currentNode):
        if (currentNode.nodeType == "leaf"):
            state = await currentNode.run()
            currentNode.state = state
            return state

        if (currentNode.nodeType == "sequence"):
            skip = False
            #if (currentNode.state == "running"):
            #    skip = True
            for i in range(len(currentNode.children)):
                if (skip):
                    if (currentNode.children[i].state == "running"):
                        skip = False
                    else:
                        continue

                temp = await Tree.runHelper(currentNode.children[i])
                if (temp == "fail"):
                    currentNode.state = "fail"
                    return "fail"
                elif (temp == "running"):
                    currentNode.state = "running"
                    return "running"
            currentNode.state = "success"
            return "success"

        elif (currentNode.nodeType == "selector"):
            for i in range(len(currentNode.children)):
                temp = await Tree.runHelper(currentNode.children[i])
                if (temp == "success"):
                    currentNode.state = "success"
                    return "success"
                elif (temp == "running"):
                    currentNode.state = "running"
                    return "running"
            currentNode.state = "fail"
            return "fail"
        else:
            for i in range(len(currentNode.children)):
                temp = await Tree.runHelper(currentNode.children[i])
                currentNode.state = temp
                return

    def clean(self, full=True):
        Tree.cleanHelper(self.root, full)

    def cleanHelper(currentNode, full):
        if (full):
            currentNode.state = "ready"
        else:
            if (currentNode.state != "running"):
                currentNode.state = "ready"
        for i in range(len(currentNode.children)):
            Tree.cleanHelper(currentNode.children[i], full)

    def printTree(self, printDescription=False, colorForBuild=True):
        print(colored(
            "\n--------------------------------------------------Tree--------------------------------------------------",
            'green'))
        print("\nMeaning of the colors:")
        if (colorForBuild):
            print(colored("Leaf Node", 'green'))
            print(colored("Selector Node", 'blue'))
            print(colored("Sequence Node\n", 'red'))
        else:
            print(colored("Success Node", 'green'))
            print(colored("Running Node", 'blue'))
            print(colored("Fail Node\n", 'red'))
        depth = 0
        Tree.printTreeHelper(self.root, depth, printDescription, colorForBuild)
        print(colored(
            "--------------------------------------------------Tree--------------------------------------------------\n",
            'red'))

    def printTreeHelper(currentNode, depth, printDescription, colorForBuild):
        spacer = ""
        for i in range(depth):
            spacer += "|\t"

        printString = "(" + currentNode.id + ")" + currentNode.nodeType
        if (printDescription):
            if (currentNode.description != None):
                printString += "(" + currentNode.description + ")"
        if (colorForBuild):
            if (currentNode.nodeType == "leaf"):
                print(spacer, colored(printString, 'green'))
            elif (currentNode.nodeType == "selector"):
                print(spacer, colored(printString, 'blue'))
            elif (currentNode.nodeType == "sequence"):
                print(spacer, colored(printString, 'red'))
            else:
                print(spacer, printString)
        else:
            if (currentNode.state == "running"):
                print(spacer, colored(printString, 'blue'))
            elif (currentNode.state == "fail"):
                print(spacer, colored(printString, 'red'))
            elif (currentNode.state == "success"):
                print(spacer, colored(printString, 'green'))
            else:
                print(spacer, printString)

        for i in range(len(currentNode.children)):
            Tree.printTreeHelper(currentNode.children[i], depth + 1, printDescription, colorForBuild)

    def getNode(self, id):
        return Tree.getNodeHelper(self.root, id)

    def getNodeHelper(currentNode, id):
        if (currentNode.id == id):
            return currentNode
        for i in range(len(currentNode.children)):
            temp = Tree.getNodeHelper(currentNode.children[i], id)
            if (temp != None):
                return temp

    def isTreeComplete(self, verbose=False):
        iscomplete = Tree.isTreeCompleteHelper(self.root, verbose)
        if (iscomplete == None):
            iscomplete = True
        if (verbose):
            print("isTreeComplete: ", iscomplete)
        return iscomplete

    def isTreeCompleteHelper(currentNode, verbose):
        if (len(currentNode.children) == 0):
            if (currentNode.nodeType != "leaf"):
                if (verbose):
                    print("ID ", currentNode.id, " should be a leaf")
                return False

        for i in range(len(currentNode.children)):
            temp = Tree.isTreeCompleteHelper(currentNode.children[i], verbose)
            if (temp != None):
                return temp


class Node:
    tree = None
    description = None
    state = None
    parent = None
    children = None
    nodeType = None
    action = None
    id = None

    def __init__(self, parent, nodeType, action=None, description=None, id=None):
        if (parent == None):
            if (nodeType != "root"):
                print("Node need parent")
                quit("program exited")
        if (parent != None):
            if (parent.nodeType == "leaf"):
                print("parent cannot be a leaf node")
                quit("program exited")

        if (id == None and parent == None):
            print("id and parent cannot be none at the same time")
            quit("program exited")

        if (nodeType == "leaf"):
            if (action == None):
                print("A leaf node need an action")
                quit("program exited")

        if (id == None):
            self.id = parent.id + str(len(parent.children))
        else:
            self.id = id

        if (nodeType != "root"):
            self.parent = parent
            parent.children.append(self)
        self.children = []

        self.nodeType = nodeType
        if (nodeType == "leaf"):
            self.action = action

        self.state = "ready"
        self.description = description

    def run(self):
        if (self.nodeType == "leaf"):
            try:
                return self.action()
            except:
                print("Cannot run function::function variable is not a function or function failed to run")
                quit("program exited")
        else:
            print("Cannot run function on a non leaf node")
            quit("program exited")