//#pragma GCC optimize ("O3")
//#pragma GCC target ("avx")

#include <iostream>
#include <fstream>
#include "Coordinator.h"
#include "IOstuff.h"
#include <string>

#define FOR(i,a,b) for(int i=a;i<b;i++)
#define rep(i,n) FOR(i,0,n)
#define ll int64_t

using namespace std;

void init(int, int, int);

Coordinator coordinator;

void tester();

int main(int argc, char *argv[]) {
    std::ios_base::sync_with_stdio(false);
    std::cin.tie(nullptr);

    init(13, 4, 200);
    //tester();

    std::cout << std::flush;

    return 0;
}

void tester() {
    NEAT aa(13, 4);

    for (int i = 0; i < aa.gencopies.size(); i++) {
        Genome& g = aa.gencopies[i];
        cout << "A " << g.getFrom() << " " << g.getTo() << " TYP " << aa.nodes[g.getFrom()].getType() << " _ "
            << aa.nodes[g.getTo()].getType() << endl;
    }

    for (const auto& n : aa.nodes) {
        cout << "TEST " << n.second.getType() << "\n";
    }
}



float test[] = { 1.f,1.f,1.f,1.f,1.f,1.f,1.f,1.f, 1.f,1.f,1.f,1.f,1.f,1.f,1.f,1.f };

void init(int numIn, int numOut, int numAI) {
    coordinator.init(numIn, numOut, numAI);
    coordinator.load("NEAT_SAVE3");

    rep(i, coordinator.neats.size()) {
        coordinator.neats[i].fitness = Helper::randi(1,1000);
    }

    rep(i, 100) {

        coordinator.evolve();
        rep(i, coordinator.neats.size()) {
            coordinator.neats[i].fitness += Helper::randi(-2, 2);
        }
        cout << "TEST " << coordinator.neats[0].nodes.size() << "\n";
        //coordinator.evolve();
        //cout << "WAT " << coordinator.neats[0].nodes.size() << " e " << coordinator.neats[0].gencopies.size() << endl;
        //Mutate::allMutations(coordinator.neats[1]);
    }
    coordinator.save(0, 3);
    //cout << "WAT " << coordinator.neats[0].nodes.size() << endl;
    //iostuff.save(coordinator.neats[0], 4);

    //coordinator.calcInput(0, test);
      //for (NEAT& neat: coordinator.neats) {
      //cout << "TEST " << neat.gencopies.size() << endl;
      //Genome& g = neat.gencopies[50];
      // cout << g.getFrom() << " " << g.getTo() << " " << g.enabled << " " << g.weight << " " << g.childNodes <<
      //      " TYP: " << neat.nodes[g.getFrom()].getType() << "\n";
      //}

}





/*std::clock_t start;
double duration;
start = std::clock();

duration = (std::clock() - start) / (double)((clock_t)1000);
std::cout << "printf: " << duration << '\n';*/