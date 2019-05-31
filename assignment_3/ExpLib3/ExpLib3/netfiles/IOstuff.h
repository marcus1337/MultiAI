#pragma once
#include "NEAT.h"
#include <stdio.h>  /* defines FILENAME_MAX */
#include <iostream>
#include <fstream>
#include <string>

class IOstuff {

public:



    static void save(NEAT& neat, int id) {
        std::ofstream myfile;
        myfile.open("NEAT_SAVE" + std::to_string(id));

        myfile << neat.numIn << " " << neat.numOut << "\n";
        myfile << neat.fitness << "\n" << neat.gencopies.size() << "\n";
        for (int i = 0; i < neat.gencopies.size(); i++) {
            myfile << neat.gencopies[i].getFrom() << " " << neat.gencopies[i].getTo() << " " << neat.gencopies[i].enabled
                << " " << neat.gencopies[i].weight << " " << neat.gencopies[i].childNodes << "\n";
        }
    }

    static NEAT load(std::string filename) {
        std::ifstream myfile(filename);
        NEAT neat(myfile);
        
        return neat;
    }

};