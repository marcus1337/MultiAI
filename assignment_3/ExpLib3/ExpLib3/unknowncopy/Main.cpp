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

int main(int argc, char *argv[]) {
    std::ios_base::sync_with_stdio(false);
    std::cin.tie(nullptr);

    init(41, 4, 50);

    std::cout << std::flush;

    return 0;
}



float test[] = { 1.f,1.f,1.f,1.f,1.f,1.f,1.f,1.f, 1.f,1.f,1.f,1.f,1.f,1.f,1.f,1.f };

void init(int numIn, int numOut, int numAI) {
    coordinator.init(numIn, numOut, numAI);

    rep(i, coordinator.neats.size()) {
        coordinator.neats[i].fitness = Helper::randi(0,1000);
    }

    //rep(i, 3000) {

      //  Mutate::allMutations(coordinator.neats[0]);
      //  Mutate::allMutations(coordinator.neats[1]);
    //}

    IOstuff iostuff;
    iostuff.save(coordinator.neats[0], 4);

    coordinator.evolve();


}





/*std::clock_t start;
double duration;
start = std::clock();

duration = (std::clock() - start) / (double)((clock_t)1000);
std::cout << "printf: " << duration << '\n';*/