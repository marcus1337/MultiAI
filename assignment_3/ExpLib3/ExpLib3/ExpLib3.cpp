

#include "stdafx.h"
#include <utility>
#include <limits.h>
#include "ExpLib3.h"
#include "netfiles/Coordinator.h"
#include <vector>


using namespace std;

Coordinator coordinator;

void init(int inputs, int outputs, int numAI)
{
    coordinator.init(inputs, outputs, numAI);
}

void calc(int index, float* inputs)
{
    coordinator.calcInput(index, inputs);
}

float* getOutput(int index)
{
    return coordinator.getOutput(index);
}

void setFitness(int index, int fitness) {
    coordinator.setFitness(index, fitness);
}

void evolve() {
    coordinator.evolve();
}

void save(int index, int fileID) {
    coordinator.save(index, fileID);
}

void load(char* filename) {
    coordinator.load(filename);
}