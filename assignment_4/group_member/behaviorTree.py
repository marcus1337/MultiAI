import threading
import time
from termcolor import colored

def main():
    print("Program started...")

    #---------------Example tree-------------------------
    #layer0
    tree = Tree(Node(parent=None, nodeType="root", id="0"))
    Node.tree = tree

    #layer1
    Node(parent=tree.getNode("0"), nodeType="selector")

    #layer2

    Node(parent=tree.getNode("00"), nodeType="selector", description="defend")
    Node(parent=tree.getNode("00"), nodeType="sequence", description="fix base")
    Node(parent=tree.getNode("00"), nodeType="sequence", description="attack enemy")

    
    #layer3
    Node(parent=tree.getNode("000"), nodeType="sequence", description="enemy army in base or close to base")
    Node(parent=tree.getNode("000"), nodeType="selector", description="control map")
    
    Node(parent=tree.getNode("001"), nodeType="sequence", description="fix bases")
    Node(parent=tree.getNode("001"), nodeType="leaf", description="expand base", action=expandBase)


    Node(parent=tree.getNode("002"), nodeType="selector", description="has good economy")
    Node(parent=tree.getNode("002"), nodeType="selector", description="has army")


    #layer4
    Node(parent=tree.getNode("0000"), nodeType="selector", description="has army")
    Node(parent=tree.getNode("0000"), nodeType="leaf", description="build army", action=buildArmy)

    Node(parent=tree.getNode("0001"), nodeType="selector", description="is not beginning of game")

    Node(parent=tree.getNode("0010"), nodeType="selector", description="creating drone")
    Node(parent=tree.getNode("0010"), nodeType="selector", description="has queen")

    Node(parent=tree.getNode("0020"), nodeType="leaf", description="build army", action=exampleAction)
    Node(parent=tree.getNode("0021"), nodeType="selector", description="Big army not on creation")

    #layer5
    Node(parent=tree.getNode("00000"), nodeType="leaf", description="back to base with army", action=backToBaseWithArmy)

    Node(parent=tree.getNode("00010"), nodeType="selector", description="early game")
    Node(parent=tree.getNode("00010"), nodeType="selector", description="mid game")
    Node(parent=tree.getNode("00010"), nodeType="selector", description="late game")

    Node(parent=tree.getNode("00100"), nodeType="selector", description="having max drones")
    Node(parent=tree.getNode("00101"), nodeType="leaf", description="create queen", action=exampleAction)

    Node(parent=tree.getNode("00210"), nodeType="selector", description="know where any expansion is")
    
    #layer6
    Node(parent=tree.getNode("000100"), nodeType="selector", description="find start base")
    Node(parent=tree.getNode("000101"), nodeType="leaf", description="control with zergling", action=exampleAction)
    Node(parent=tree.getNode("000102"), nodeType="leaf", description="control with mutalisk", action=exampleAction)

    Node(parent=tree.getNode("001000"), nodeType="leaf", description="create drone", action=exampleAction)

    Node(parent=tree.getNode("002100"), nodeType="selector", description="has army at expansion")

    #layer7
    Node(parent=tree.getNode("0001000"), nodeType="leaf", description="control with drone", action=controlDrone)

    Node(parent=tree.getNode("0021000"), nodeType="leaf", description="rush expansion", action=exampleAction)
    #---------------Example tree-------------------------
    
    completeTree = tree.isTreeComplete(verbose=True)
    if(completeTree):
        tree.printTree(printDescription = True, colorForBuild=True)

        tree.printTree(printDescription = True, colorForBuild=False)
        tree.run()
        tree.printTree(printDescription = True, colorForBuild=False)
        tree.clean(full=False)
        tree.printTree(printDescription = True, colorForBuild=False)

#------for multithreading purposes------
'''
    threading.Thread(target=test).start()
    threading.Thread(target=test).start()

def test():
    for i in range(100):
        print(i)
        time.sleep(1)
'''
#------for multithreading purposes------


#---------example of getting success-----------
'''
def controlDrone():
    print("control with drone")
    return "success"

def buildArmy():
    print("buildArmy")
    return "fail"
'''
#---------example of getting success-----------



#---------example of getting running-----------

def controlDrone():
    print("control with drone")
    return "fail"

def buildArmy():
    print("buildArmy")
    return "running"

#---------example of getting running-----------



def exampleAction():
    print("exampleAction")
    return "fail"

def expandBase():
    print("expandBase")
    return "fail"


def backToBaseWithArmy():
    print("backToBaseWithArmy")
    return "success"

def defend():
    print("Defend")
    return "fail"


def attack():
    print("Attack")
    return "fail"

class Tree:
    root = None
    def __init__(self, root):
        self.root = root

    def run(self):
        Tree.runHelper(self.root)

    def runHelper(currentNode):
        if(currentNode.nodeType == "leaf"):
            state = currentNode.run()
            currentNode.state = state
            return state

        if(currentNode.nodeType == "sequence"):
            skip = False
            if(currentNode.state == "running"):
                skip = True
            for i in range(len(currentNode.children)):
                if(skip):
                    if(currentNode.children[i].state == "running"):
                        skip = False
                    else:
                        continue

                temp = Tree.runHelper(currentNode.children[i])
                if(temp == "fail"):
                    currentNode.state = "fail"
                    return "fail"
                elif(temp == "running"):
                    currentNode.state = "running"
                    return "running"

        elif(currentNode.nodeType == "selector"):
            for i in range(len(currentNode.children)):
                temp = Tree.runHelper(currentNode.children[i])
                if(temp == "success"):
                    currentNode.state = "success"
                    return "success"
                elif(temp == "running"):
                    currentNode.state = "running"
                    return "running"
        else:
            for i in range(len(currentNode.children)):
                temp = Tree.runHelper(currentNode.children[i])
                currentNode.state = temp
                return
        currentNode.state = "fail"
        return "fail"
    
    def clean(self,full=True):
        Tree.cleanHelper(self.root, full)

    def cleanHelper(currentNode, full):
        if(full):
            currentNode.state = "ready"
        else:
            if(currentNode.state != "running"):
                currentNode.state = "ready"
        for i in range(len(currentNode.children)):
            Tree.cleanHelper(currentNode.children[i], full)



    def printTree(self, printDescription=False, colorForBuild=True):
        print(colored("\n--------------------------------------------------Tree--------------------------------------------------",'green'))
        print("\nMeaning of the colors:")
        if(colorForBuild):
            print(colored("Leaf Node", 'green'))
            print(colored("Selector Node", 'blue'))
            print(colored("Sequence Node\n", 'red'))
        else:
            print(colored("Success Node", 'green'))
            print(colored("Running Node", 'blue'))
            print(colored("Fail Node\n", 'red'))
        depth = 0
        Tree.printTreeHelper(self.root, depth, printDescription, colorForBuild)
        print(colored("--------------------------------------------------Tree--------------------------------------------------\n",'red'))

    def printTreeHelper(currentNode, depth, printDescription, colorForBuild):
        spacer = ""
        for i in range(depth):
            spacer += "|\t"

        printString = "("+ currentNode.id + ")" + currentNode.nodeType
        if(printDescription):
            if(currentNode.description != None):
                printString += "("+ currentNode.description + ")"
        if(colorForBuild):
            if(currentNode.nodeType == "leaf"):
                print(spacer, colored(printString, 'green'))
            elif(currentNode.nodeType == "selector"):
                print(spacer, colored(printString, 'blue'))
            elif(currentNode.nodeType == "sequence"):
                print(spacer, colored(printString, 'red'))
            else:
                print(spacer, printString)
        else:
            if(currentNode.state == "running"):
                print(spacer, colored(printString, 'blue'))
            elif(currentNode.state == "fail"):
                print(spacer, colored(printString, 'red'))
            elif(currentNode.state == "success"):
                print(spacer, colored(printString, 'green'))
            else:
                print(spacer, printString)
        
        for i in range(len(currentNode.children)):
            Tree.printTreeHelper(currentNode.children[i], depth+1, printDescription, colorForBuild)

    def getNode(self, id):
        return Tree.getNodeHelper(self.root, id)

    def getNodeHelper(currentNode, id):
        if(currentNode.id == id):
            return currentNode
        for i in range(len(currentNode.children)):
            temp = Tree.getNodeHelper(currentNode.children[i], id)
            if(temp != None):
                return temp

    def isTreeComplete(self, verbose=False):
        iscomplete = Tree.isTreeCompleteHelper(self.root, verbose)
        if(iscomplete == None):
            iscomplete = True
        if(verbose):
            print("isTreeComplete: ", iscomplete)
        return iscomplete

    def isTreeCompleteHelper(currentNode, verbose):
        if(len(currentNode.children) == 0):
            if(currentNode.nodeType != "leaf"):
                if(verbose):
                    print("ID ", currentNode.id, " should be a leaf")
                return False

        for i in range(len(currentNode.children)):
            temp = Tree.isTreeCompleteHelper(currentNode.children[i], verbose)
            if(temp != None):
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
        if(parent == None):
            if(nodeType != "root"):
                print("Node need parent")
                quit("program exited")
        if(parent != None):
            if(parent.nodeType == "leaf"):
                print("parent cannot be a leaf node")
                quit("program exited")
        
        if(id == None and parent == None):
            print("id and parent cannot be none at the same time")
            quit("program exited")
        
        if(nodeType == "leaf"):
            if(action == None):
                print("A leaf node need an action")
                quit("program exited")

        if(id == None):
            self.id = parent.id + str(len(parent.children))
        else:
            self.id = id

        if(nodeType != "root"):
            self.parent = parent
            parent.children.append(self)
        self.children = []

        self.nodeType = nodeType
        if(nodeType == "leaf"):
            self.action = action

        self.state = "ready"
        self.description = description

    def run(self):
        if(self.nodeType == "leaf"):
            try:
                return self.action()
            except:
                print("Cannot run function::function variable is not a function")
                quit("program exited")
        else:
            print("Cannot run function on a non leaf node")
            quit("program exited")

main()