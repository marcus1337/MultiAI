import gym

import time
import sys
import ctypes
import numpy

mFile = 'C:\\Users\\Marcus\\PycharmProjects\\multiagent\\tenstest\\ExpLib3.dll'
env = gym.make('BipedalWalker-v2')
env.reset()
goal_steps = 1600

numIn = 13
numOut = 4
numAI = 800

num_games = 100000000
fitnessArr = numpy.zeros(numAI)
fitnessArr = fitnessArr.astype(int)
######################

def calc(NEAT, index , pyarr):
     arr = (ctypes.c_float * len(pyarr))(*pyarr)
     NEAT.calc(index, arr)
     res = NEAT.getOutput(index)
     pres = []
     for i in range(0,numOut):
         pres.append(res[i])
     return pres

def improve_fitness(NEAT, fitnessArr):
    for index in range(0, numAI):
        NEAT.setFitness(index, ctypes.c_int(fitnessArr[index]))

    NEAT.evolve()

def loadFiles():
    NEAT = None
    try:
        NEAT = ctypes.WinDLL(mFile)
        NEAT.getOutput.restype = ctypes.POINTER(ctypes.c_float)
        NEAT.init.restype = None
        NEAT.evolve.restype = None
        NEAT.calc.restype = None
        NEAT.setFitness.restype = None
        NEAT.save.restype = None
        NEAT.load.restype = None

        NEAT.getOutput.argtypes = [ctypes.c_int]
        NEAT.init.argtypes = [ctypes.c_int, ctypes.c_int, ctypes.c_int]
        NEAT.evolve.argtypes = []
        NEAT.save.argtypes = [ctypes.c_int, ctypes.c_int]
        NEAT.load.argtypes = [ctypes.c_char_p]
        NEAT.calc.argtypes = [ctypes.c_int, ctypes.POINTER(ctypes.c_float)]
        NEAT.setFitness.argtypes = [ctypes.c_int, ctypes.c_int]

        NEAT.init(numIn, numOut, numAI)

        pyarr = b'NEAT_SAVE4'
        arr = (ctypes.c_char * len(pyarr))(*pyarr)
        #NEAT.load(arr)

    except OSError as e:
        print("ERROR: %s" % e)
        sys.exit(1)

    return NEAT


def getObservationData(observation):
    res = []
    for i in range(0,numIn):
        res.append(observation[i])

    return res

def getBestIndex():
    bestScoreFound = -9999999
    bestIndex = -1
    for i in range(0, numAI):
        if (fitnessArr[i] > bestScoreFound):
            bestScoreFound = fitnessArr[i]
            bestIndex = i
    return bestIndex, bestScoreFound

def remakeAction(action):
    res = [0,0,0,0]
    for i in range(0,4):
        if (action[i] >= 0):
            res[i] = 2
        else:
            res[i] = -2
    return res

def staleness(action):
    totalValue = 0
    for i in range(0,4):
        if (action[i] <= 0.5 and action[i] >= -0.5 ):
            totalValue += 1
    totalValue = totalValue * 30
    return totalValue
def staleness2(obs):
    totalVal = 0
    if (obs[2] >= 0 and obs[2] <= 0.05):
        totalVal += 50
    if (obs[2] < 0):
        totalVal += 100

    return totalVal

def train_nets(NEAT):
    for gameNum in range(num_games):
        start = time.time()
        for index in range(0, numAI):
            score = 0
            observation = env.reset()
            countSteps = 1
            total = 0
            for _ in range(goal_steps):
                countSteps = countSteps + 1
                #if index == 0 and gameNum > 100:
                env.render()
                observ = getObservationData(observation)
                action = calc(NEAT,index,observ)
                observation, reward, done, info = env.step(action)
                total += observation[2]
                #print("TOTAL " + str(total) + "  time: " + str(time.time() - start) + " step " + str(countSteps))
                score += observation[2]
                #score -= staleness2(observation)

                #print("SCORE " + str(score))
                if (done):
                    #score -= int(300000000/countSteps)
                    break
            #env.close()
            #print("SCORE " + str(int(score)))
            fitnessArr[index] = int(score)


        if gameNum % 2 == 0:
            bestIndex, bestScoreF = getBestIndex()
            NEAT.save(bestIndex, gameNum)
            print("SAVED: " + str(bestIndex) + " SCORE: " + str(bestScoreF))
            print(fitnessArr)

        improve_fitness(NEAT, fitnessArr)
        #print(fitnessArr)
        print("DONE " + str(gameNum) + "  STEPTIME: " + str(time.time() - start))


def initStuff():
    NEAT = loadFiles()

    return NEAT



def run_code():
   NEAT = initStuff()
   train_nets(NEAT)
   env.close()


if __name__ == '__main__':
    run_code()
