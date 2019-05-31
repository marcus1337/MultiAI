#pragma once
#include "NEAT.h"
#include <algorithm>
#include "Innovator.h"
#include <iostream>
#include <random>
#include "Mutate.h"
#include "HelperFuncs.h"

class Speciator {

    // std::vector<Genome> parent1;
    // std::vector<Genome> parent2;
    int numOut, numIn, numAI;

public:
    std::default_random_engine rng;

    Speciator() {
        rng = std::default_random_engine{};
        
    }
    void init(int _numIn, int _numOut, int _numAI) {
        numOut = _numOut;
        numIn = _numIn;
        numAI = _numAI;
    }

    struct Specie {
        int id;
        std::vector<NEAT*> neats;
        int topFitness = 0;

        Specie() : id(-1) {}
        Specie(int _id) : id(_id) {
        }

        bool operator < (const Specie &right) const
        {
            return id < right.id;
        }
        bool operator==(const Specie& rhs) const { return this->id == rhs.id; }
    };

    std::vector<Specie> pool;
    int specieNum;

    std::vector<NEAT> children;

    void speciate(std::vector<NEAT>& neats) {


        for (NEAT& neat : neats) {

            std::sort(neat.gencopies.begin(), neat.gencopies.end());
        }
        
        specieNum = 0;
        pool.clear();
        children.clear();
        Specie spec(specieNum);
        spec.topFitness = neats[0].fitness;
        specieNum++;
        spec.neats.push_back(&neats[0]);
        pool.push_back(spec);

        for (size_t i = 1; i < neats.size(); i++) {
            addToSpecies(neats[i]);
        }

        removeStaleSpecies();
        cullSpecies();

        //std::cout << "WAT " << neats.size() << " , " << neats[0].fitness << std::endl;

        newGeneration();
        neats = std::vector<NEAT>(children);
    }

    template <class T> void swap(T& a, T& b)
    {
        T c(a); a = b; b = c;
    }

    void crossOver(NEAT* n1, NEAT* n2) {
        NEAT child(numIn, numOut);

        if (n2->fitness > n1->fitness) { //n1 has better fit
            swap<NEAT*>(n1, n2);
        }

        for (auto const& x : n1->nodes) {
            Node nod(x.second.getID());
            child.nodes[nod.getID()] = nod;
        }
        for (auto const& x : n2->nodes) {
            Node nod(x.second.getID());
            child.nodes[nod.getID()] = nod;
        }

        std::vector<Genome>& g1 = n1->gencopies;
        std::vector<Genome>& g2 = n2->gencopies;

        if (n1->fitness == n2->fitness) {

            for (int i = 0, j = 0;;) {
                if (i == g1.size() - 1) {
                    for (int jj = j; jj < g2.size(); jj++) {
                        child.addGeneNoLoop(g2[jj]);
                    }

                    break;
                }
                if (j == g2.size() - 1) {
                    for (int ii = i; ii < g1.size(); ii++) {
                        child.addGeneNoLoop(g1[ii]);
                    }

                    break;
                }

                if (g1[i].getID() == g2[j].getID()) {
                    bool b = Helper::randomBool();
                    if (b) {
                        child.addGene(g1[i].getFrom(), g1[i].getTo());
                    }
                    else {
                        child.addGene(g2[i].getFrom(), g2[i].getTo());
                    }
                    i++;
                    j++;
                }else if (g1[i].getID() < g2[j].getID()) {
                    child.addGeneNoLoop(g1[i]);
                    i++;
                }
                else if (g1[i].getID() > g2[j].getID()) {
                    child.addGeneNoLoop(g2[j]);
                    j++;
                }
            }
        }
        else {
            child.copyPointer(n1);
            
            for (int i = 0, j = 0;;) {
                if (i == g1.size() - 1) {
                    for (int jj = j; jj < g2.size(); jj++) {
                        child.addGeneNoLoop(g2[jj]);
                    }
                    break;
                }
                if (j == g2.size() - 1) {
                    break;
                }

                if (g1[i].getID() == g2[j].getID()) {
                    i++;
                    j++;
                }
                else if (g1[i].getID() < g2[j].getID()) {
                    i++;
                }
                else if (g1[i].getID() > g2[j].getID()) {
                    child.addGeneNoLoop(g2[j]);
                    j++;
                }
            }

        }

        child.removeRedundants();
        Mutate::allMutations(child);
        children.push_back(child);

    }

    static constexpr float crossChance = 0.80f;

    void newGeneration() {

        pool.erase(std::remove_if(pool.begin(), pool.end(),
                [](const Specie& o) { return o.neats.size() == 0; }), pool.end());

        while(true)
        for (Specie& spec : pool) {
           
            if (children.size() == numAI)
                return;

            if (spec.neats.empty())
                continue;
            if (spec.neats.size() == 1) {
                NEAT child(numIn, numOut);
                child.copyPointer(spec.neats[0]);
                Mutate::allMutations(child);
                children.push_back(child);
                continue;
            }
            std::shuffle(std::begin(spec.neats), std::end(spec.neats), rng);
            for (int i = 0; i < spec.neats.size() - 1; i++) {
                float randf = Helper::randf(0,100);

                if (randf > crossChance) {
                    NEAT child(numIn, numOut);
                    child.copyPointer(spec.neats[i]);
                    Mutate::allMutations(child);
                    children.push_back(child);
                }
                else {
                    crossOver(std::ref(spec.neats[i]), std::ref(spec.neats[i + 1]));
                }
               
                if (children.size() == numAI)
                    return;
            }

        }

    }

    void removeStaleSpecies() {
        std::vector<Specie> survived;
  
        std::sort(pool.begin(), pool.end(), [](const Specie& lhs, const Specie& rhs)
        {
            return lhs.topFitness > rhs.topFitness;
        });
        for (size_t i = 0; i < pool.size() && i < 200; i++) {
            survived.push_back(pool[i]);
        }
        pool = survived;

    }

    void cullSpecies() {

        for (Specie& spec : pool) {
            std::sort(spec.neats.begin(), spec.neats.end(), [](const NEAT* lhs, const NEAT* rhs) //smallest fitness in beginning
            {
                return lhs->fitness > rhs->fitness;
            });

            int remaining = spec.neats.size() / 2;
            std::vector<NEAT*> survivors;
            if (remaining == 0) {
                survivors.push_back(spec.neats[0]);
            }
                
            for (int i = 0; i < spec.neats.size() - remaining; i++) {
                survivors.push_back(spec.neats[i]);
            }

            spec.neats = survivors;
            
        }

    }

    void addToSpecies(NEAT& neat) {
        bool foundSpecies = false;
        for (int i = 0; i < specieNum; i++) {
            NEAT& tmpNeat = *pool[i].neats[0];
            if (sameSpecie(neat, tmpNeat)) {
                foundSpecies = true;
                pool[i].neats.push_back(&neat);
                if (pool[i].topFitness < neat.fitness)
                    pool[i].topFitness = neat.fitness;

                break;
            }
        }
        if (!foundSpecies) {
            specieNum++;
            Specie spec(specieNum);
            spec.topFitness = neat.fitness;
            spec.neats.push_back(&neat);
            pool.push_back(spec);
        }
    }

    static constexpr float c1 = 0.5f;
    static constexpr float c2 = 0.15f;

    bool sameSpecie(NEAT& n1, NEAT& n2) {
        //int LN = n1.gencopies.size() > n2.gencopies.size() ? n1.gencopies.size() : n2.gencopies.size();
        float dd = c1*(float)disjointDiff(n1,n2);
        float dw = c2*weightDiff(n1,n2);
        return (dd+dw) < 20.0f;
    }


    float weightDiff(NEAT& n1, NEAT& n2) {
        float res = 0;
        std::vector<Genome>& g1 = n1.gencopies;
        std::vector<Genome>& g2 = n2.gencopies;

        for (int i = 0, j = 0;;) {
            if (i == g1.size() - 1) {
                break;
            }
            if (j == g2.size() - 1) {
                break;
            }

            if (g1[i].getID() == g2[j].getID()) {
                float w1 = g1[i].weight;
                float w2 = g2[j].weight;
                float diff = w1 > w2 ? abs(w1 - w2) : abs(w2 - w1);
                res += diff;
                i++;
                j++;
            }else if (g1[i].getID() < g2[j].getID()) {
                i++;
            }
            else if (g1[i].getID() > g2[j].getID()) {
                j++;
            }
        }

        return res;
    }

    int disjointDiff(NEAT& n1, NEAT& n2) {
        int res = 0;
        std::vector<Genome>& g1 = n1.gencopies;
        std::vector<Genome>& g2 = n2.gencopies;

        for (int i = 0, j = 0;;) {
            if (i == g1.size() - 1) {
                res += g2.size() - i;
                break;
            }
            if (j == g2.size() - 1) {
                res += g1.size() - j;
                break;
            }

            if (g1[i].getID() == g2[j].getID()) {
                i++;
                j++;
            }else if (g1[i].getID() < g2[j].getID()) {
                i++;
                res++;
            }
            else if (g1[i].getID() > g2[j].getID()) {
                j++;
                res++;
            }
        }

        return res;
    }


};