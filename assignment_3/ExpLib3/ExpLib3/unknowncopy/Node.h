#pragma once
#include <list>
#include <set>
#include "Genome.h"

class Node {

    int nodTyp, id;

public:
    static const int INPUT = 1, HIDDEN = 2, OUTPUT = 3;

    int getType() const {
        return nodTyp;
    }

    int getID() const {
        return id;
    }

    std::set<Genome> genomes;
    float value = 0;
 
    Node() : nodTyp(0), id(0){}

    Node(int _id, int _nodTyp = HIDDEN) : id(_id), nodTyp(_nodTyp) {

    }

    Node(const Node &n2) : id(n2.id), nodTyp(n2.nodTyp), genomes(n2.genomes) {
    }

    bool operator< (const Node &right) const
    {
        return id < right.id;
    }
    bool operator==(const Node& rhs) const { return this->id == rhs.id; }

};