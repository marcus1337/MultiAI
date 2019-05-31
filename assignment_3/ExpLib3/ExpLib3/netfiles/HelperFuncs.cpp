#include "HelperFuncs.h"
#include "NEAT.h"
#include <iostream>

bool Helper::hasLoop(NEAT& neat) {
    std::set<int> white;
    std::set<int> grey;
    std::set<int> black;

    for (size_t i = 0; i < neat.nodes.size(); i++) {
        white.insert(neat.nodes[i].getID());
    }

    while (!white.empty()) {
        auto it = white.begin();
        int current = *it;
        if (dfs(current, white, grey, black, neat)) {
            return true;
        }
    }

    return false;
}

bool Helper::hasLoopEasy(NEAT& neat, int start) {
    std::set<int> white;
    std::set<int> grey;
    std::set<int> black;
    white.insert(start);

    while (!white.empty()) {
        auto it = white.begin();
        int current = *it;
        if (dfs(current, white, grey, black, neat)) {
            return true;
        }
    }

    return false;
}

bool Helper::dfs(int current, std::set<int>& white, std::set<int>& gray, std::set<int>& black, NEAT& neat) {
    move_vertex(current, white, gray);
    for (const Genome& genome : neat.nodes[current].genomes) {
        if (black.find(genome.getTo()) != black.end()) {
            continue;
        }
        if (gray.find(genome.getTo()) != gray.end()) {
            return true;
        }
        if (dfs(genome.getTo(), white, gray, black, neat)) {
            return true;
        }
    }

    move_vertex(current, gray, black);
    return false;
}


void Helper::move_vertex(int vertex, std::set<int>& source_set, std::set<int>& destination_set) {
    source_set.erase(vertex);
    destination_set.insert(vertex);
}


void Helper::topologicalSortUtil(int v, std::vector<bool>& visited, std::stack<int>& topstack, NEAT& neat) {
    visited[v] = true;
    std::list<int>::iterator i;

    for (const Genome& genome : neat.nodes[v].genomes) {

        if (!visited[genome.getTo()])
            topologicalSortUtil(genome.getTo(), visited, topstack, neat);
    }

    topstack.push(v);
}

std::stack<int> Helper::topSort(NEAT& neat) {
    std::stack<int> topstack;

    std::vector<bool> visited(neat.nodes.size()*100 +neat.gencopies.size()*20+8000, false);

    for (const auto& n : neat.nodes)
    {
        int id = n.second.getID();
        if(!visited[id]) {
            topologicalSortUtil(id, visited, topstack, neat);
        }
    }

    return topstack;
}