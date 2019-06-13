#pragma once
#include "HelperFuncs.h"
#include "Innovator.h"

class Genome {
    int from, to, id;

public:

    bool enabled;

    int getFrom() const {
        return from;
    }

    int getTo() const {
        return to;
    }

    int getID() {
        return id;
    }

    float weight;
    int childNodes = 0; //How many times a gene has been split into two new genes + node.

    Genome() : id(-1) {}

    Genome(int _from, int _to) : 
        from(_from), to(_to), id(Innovator::getInstance().getNum(_from, _to)), enabled(true) {
        weight = Helper::randf(-2.f,2.f);
    }

    Genome(int _from, int _to, bool _enabled, float _weight, int _childNodes) :
        from(_from), to(_to), id(Innovator::getInstance().getNum(_from, _to)), enabled(_enabled), weight(_weight), childNodes(_childNodes) {
        
    }

    //bool enabled, float weight, int childnodes
    //Genome(const Genome &g2) : from(g2.from), to(g2.to), id(g2.id), weight(g2.weight) {}

    bool operator < (const Genome &right) const
    {
        return id < right.id;
    }
    bool operator==(const Genome& rhs) const { return this->id == rhs.id; }

};