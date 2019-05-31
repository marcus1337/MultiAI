import sc2
from termcolor import colored
#from TREE import DroneHandler, BuildHandler, WorkerHandler
from TREE import FixBase, AttackEnemy, DefendBase


class Strategies(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot
        self.fixBase = FixBase.FixBase(self.bot)
        self.attackEnemy = AttackEnemy.AttackEnemy(self.bot)
        self.defendBase = DefendBase.DefendBase(self.bot)
        #self.droneHandler = DroneHandler.DroneHandler(self.bot)
        #self.buildHandler = BuildHandler.BuildHandler(self.bot)
        #self.workerHandler = WorkerHandler.WorkerHandler(self.bot)

    def getTree(self):
        tree = Tree(Node(parent=None, nodeType="root", id="0"))
        Node.tree = tree

        Node(parent=tree.getNode("0"), nodeType="selector")

        
        Node(parent=tree.getNode("00"), nodeType="selector", description="Fix base")

        

        Node(parent=tree.getNode("000"), nodeType="leaf", description="Debuging purposes", action=self.fixBase.debug)

        #Node(parent=tree.getNode("000"), nodeType="leaf", description="Defend Base", action=self.defendBase.moveArmyToRamp)

        Node(parent=tree.getNode("000"), nodeType="leaf", description="Handle idle workers", action=self.fixBase.handleIdleWorkers)
        Node(parent=tree.getNode("000"), nodeType="leaf", description="Set workers on extractor", action=self.fixBase.setWorkersOnExtractor)
        Node(parent=tree.getNode("000"), nodeType="leaf", description="Export workers to other base", action=self.fixBase.exportWorkersToOtherBase)

    

        Node(parent=tree.getNode("000"), nodeType="leaf", description="Build overlords", action=self.fixBase.buildOverlord)

        Node(parent=tree.getNode("000"), nodeType="selector", description="Handle queen")
        Node(parent=tree.getNode("0005"), nodeType="leaf", description="Inject bases", action=self.fixBase.injectHatchery)
        Node(parent=tree.getNode("0005"), nodeType="leaf", description="Build queens", action=self.fixBase.buildQueen)

        Node(parent=tree.getNode("000"), nodeType="leaf", description="Build extractor", action=self.fixBase.buildExtractor)

        
        

        Node(parent=tree.getNode("000"), nodeType="leaf", description="Build drones", action=self.fixBase.buildDrone)
        Node(parent=tree.getNode("000"), nodeType="leaf", description="Build roach warren", action=self.fixBase.buildRoachWarren)
        Node(parent=tree.getNode("000"), nodeType="leaf", description="Build spawningpool", action=self.fixBase.buildSpawningpool)
        Node(parent=tree.getNode("000"), nodeType="leaf", description="Expand base", action=self.fixBase.expandBase)
        #Node(parent=tree.getNode("000"), nodeType="leaf", description="Mutate to lair", action=self.fixBase.mutateToLair)
        
        Node(parent=tree.getNode("00"), nodeType="selector", description="Attack enemy")
        Node(parent=tree.getNode("001"), nodeType="leaf", description="Build roach army", action=self.attackEnemy.buildRoach)
        Node(parent=tree.getNode("001"), nodeType="leaf", description="attack enemy", action=self.attackEnemy.attackEnemyBase)
        
        #Node(parent=tree.getNode("00"), nodeType="selector")

        
        

        #Node(parent=tree.getNode("0"), nodeType="sequence")
        #Node(parent=tree.getNode("00"), nodeType="leaf", description="manage misc units", action=self.workerHandler.update)
        #Node(parent=tree.getNode("00"), nodeType="leaf", description="expand base", action=self.buildHandler.update)
        #Node(parent=tree.getNode("00"), nodeType="leaf", description="build misc units", action=self.droneHandler.update)

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
            if (currentNode.state == "running"):
                skip = True
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

    async def run(self):
        if (self.nodeType == "leaf"):
            try:
                return await self.action()
            except:
                print("Cannot run function::function variable is not a function or function failed to run")
                quit("program exited")
        else:
            print("Cannot run function on a non leaf node")
            quit("program exited")