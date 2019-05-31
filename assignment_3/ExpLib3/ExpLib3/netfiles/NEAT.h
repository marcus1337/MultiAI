#pragma once
#include <vector>
#include <map>
#include <tuple>
#include "Node.h"
#include "Innovator.h"
#include "Mutate.h"
#include <set>

#include <iostream>
#include <functional>
#include <list>


class NEAT {
public:
    int numIn, numOut;

    bool hasLoop() {
        return Helper::hasLoop(*this);
    }
    int fitness;

    std::map<int, Node> nodes;
    std::vector<Genome> gencopies;
    std::map<std::pair<int, int>, bool> busyEdges;
    std::vector<float> outputs;

    void updateGene(Genome updatedGene) {
        Node& nowNode = nodes[updatedGene.getFrom()];
        auto found = nowNode.genomes.find(updatedGene);
        nowNode.genomes.insert(updatedGene);
    }

    void topSortCalc(float* inputs) {
        for (int i = 0; i < numIn; i++) {
            nodes[i].value = inputs[i];
        }

        std::stack<int> topstack = Helper::topSort(*this);

        while (!topstack.empty())
        {
            int nowID = topstack.top();
            topstack.pop();

            for (const Genome& genome : nodes[nowID].genomes) {
                if (!genome.enabled)
                    continue;
                int to = genome.getTo();
                nodes[to].value += nodes[nowID].value*genome.weight;
            }
        }

        for (int i = numIn; i < numIn + numOut; i++) {
            outputs[i - numIn] = nodes[i].value;
        }
        reset();
    }

    void reset() {
        for (auto& n : nodes)
        {
            n.second.value = 0;
        }
    }

    void copyPointer(NEAT* np) {
        if (np == nullptr)
            return;
        nodes = np->nodes;
        gencopies = np->gencopies;
        busyEdges = np->busyEdges;
        outputs = np->outputs;
        numIn = np->numIn;
        numOut = np->numOut;
        fitness = np->fitness;
    }

    NEAT() :numIn(-1), numOut(-1) {}

    NEAT(std::istream& stream) {
        /*        myfile << neat.numIn << " " << neat.numOut << "\n";
        myfile << neat.fitness << "\n" << neat.gencopies.size() << "\n";
        for (int i = 0; i < neat.gencopies.size(); i++) {
            myfile << neat.gencopies[i].getFrom() << " " << neat.gencopies[i].getTo() << " " << neat.gencopies[i].enabled
                << " " << neat.gencopies[i].weight << " " << neat.gencopies[i].childNodes << "\n";
        }*/
        stream >> numIn >> numOut >> fitness;
        outputs.resize(numOut);
        int totalSize;
        stream >> totalSize;
        initBaseNodes();

        for (int i = 0; i < totalSize; i++) {
            int from, to, childnodes;
            bool enabled;
            float weight;
            stream >> from >> to >> enabled >> weight >> childnodes;
            if (!Helper::mapContains<int, Node>(nodes, from)) {
                nodes[from] = Node(from);
            }
            if (!Helper::mapContains<int, Node>(nodes, to)) {
                nodes[to] = Node(to);
            }

            if (nodes[from].getType() == Node::INPUT && nodes[to].getType() == Node::OUTPUT) {
                Genome genome(from, to, enabled, weight, childnodes);
                updateGene(genome);
            }
            else {
                addGene(from, to, enabled, weight, childnodes);
            }

        }

    }

    void initBaseNodes() {
        for (int i = 0; i < numIn; i++) {
            Node node(i, Node::INPUT);
            nodes[i] = node;
        }

        for (int i = 0; i < numOut; i++) {
            Node node(i + numIn, Node::OUTPUT);
            nodes[i + numIn] = node;
        }
    }


    NEAT(int _numIn, int _numOut) : numIn(_numIn), numOut(_numOut), fitness(0), outputs(_numOut) {
        initBaseNodes();

        for (int i = 0; i < numIn; i++) {
            for (int j = numIn; j < numIn + numOut; j++) {
                int from = nodes[i].getID();
                int to = nodes[j].getID();
                addGene(from, to);
            }
        }

    }

    void addGene(int from, int to, bool enabled, float weight, int childnodes) {
        Genome gene(from, to, enabled, weight, childnodes);
        nodes[from].genomes.insert(gene);
        gencopies.push_back(gene);
        busyEdges[std::make_pair(from, to)] = true;
        busyEdges[std::make_pair(to, from)] = true;
    }

    void addGene(int from, int to) {
        Genome gene(from, to);
        nodes[from].genomes.insert(gene);
        gencopies.push_back(gene);
        busyEdges[std::make_pair(from, to)] = true;
        busyEdges[std::make_pair(to, from)] = true;
    }

    void addGeneNoLoop(Genome gene) { //can cause loops, should be excess gene before add
        int from = gene.getFrom();
        int to = gene.getTo();
        if (Helper::mapContains<std::pair<int, int>, bool>(busyEdges, std::make_pair(from, to))) {
            return;
        }

        if (!Helper::mapContains<int, Node>(nodes, from)) {
            nodes[from] = Node(from);
        }
        if (!Helper::mapContains<int, Node>(nodes, to)) {
            nodes[to] = Node(to);
        }
        if (!Mutate::isCircle(*this, from, to)) {
            addGene(from, to, gene.enabled, gene.weight, gene.childNodes);
        }
    }

    void removeRedundants() { //removes edges and nodes with dead ends
        std::set<int> badFroms;

        for (const auto& x : nodes) {
            if (x.second.genomes.size() == 0 && !(x.second.getType() == Node::OUTPUT)
                && !(x.second.getType() == Node::INPUT)) {
                badFroms.insert(x.second.getID());
            }
        }
        std::set<int> toBeRemoved;
        for (int i = 0; i < gencopies.size(); i++) {
            int from = gencopies[i].getFrom();
            int to = gencopies[i].getTo();
            const bool is_in = (badFroms.find(from) != badFroms.end()) || (badFroms.find(to) != badFroms.end());
            if (is_in) {
                toBeRemoved.insert(i);
                nodes[from].genomes.erase(gencopies[i]);
            }
        }
        std::vector<Genome> savedCopies;

        for (int i = 0; i < gencopies.size(); i++) {
            bool is_in = toBeRemoved.find(i) != toBeRemoved.end();
            if (!is_in) {
                savedCopies.push_back(gencopies[i]);
            }
        }

        gencopies = savedCopies;

        for (int x : badFroms) {
            nodes.erase(x);
        }

    }


};