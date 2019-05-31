#pragma once
#include <cstdlib>
#include <ctime>
#include "Innovator.h"
#include "Mutate.h"
#include "NEAT.h"
#include <list>
#include <tuple>
#include <vector>
#include "Speciator.h"
#include "IOstuff.h"
#include <string>

class Coordinator {

public:
    std::vector<NEAT> neats;
    int _numAI, _numOut, _numIn;

    Speciator speciator;
    int generation;

    void init(int numIn, int numOut, int numAI) {
        srand(static_cast <unsigned> (time(0)));
        generation = 1;
        _numAI = numAI;
        _numOut = numOut;
        _numIn = numIn;
        Innovator::getInstance().reset();
        for (int i = 0; i <= numIn + numOut; i++)
            Innovator::getInstance().getAnyNodeNum();

        for (int i = 0; i < numAI; i++) {
            neats.push_back(NEAT(numIn, numOut));
        }
        speciator.init(numIn, numOut, numAI);
    }

    void evolve() {
        Mutate::mutationrate = 0.8f;
        if (generation > 10 && generation < 40) {
            Mutate::mutationrate = 0.5f;
        }
        if (generation > 40 && generation < 80) {
            Mutate::mutationrate = 0.2f;
        }
        if (generation > 80) {
            Mutate::mutationrate = 0.02f;
        }

        generation++;
        speciator.speciate(neats);
    }

    void calcInput(int index, float* inputs) {
        neats[index].topSortCalc(inputs);
    }

    float* getOutput(int index) {
        return neats[index].outputs.data();
    }

    void setFitness(int index, int fitness) {
        neats[index].fitness = fitness;
    }

    void save(int index, int fileID) {
        IOstuff::save(neats[index], fileID);
    }

    void load(char* filename) {
        NEAT neat = IOstuff::load(filename);
        neats.clear();
        for (int i = 0; i < _numAI; i++) {
            neats.push_back(neat);
        }
    }

};