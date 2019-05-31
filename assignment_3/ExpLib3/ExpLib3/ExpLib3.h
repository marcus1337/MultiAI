#pragma once

#ifdef EXPLIB3_EXPORTS
#define EXPLIB3_API __declspec(dllexport)
#else
#define EXPLIB3_API __declspec(dllimport)
#endif


extern "C" EXPLIB3_API void init(int inputs, int outputs, int numAI);

extern "C" EXPLIB3_API void calc(int index, float* inputs);

extern "C" EXPLIB3_API float* getOutput(int index);

extern "C" EXPLIB3_API void setFitness(int index, int fitness);

extern "C" EXPLIB3_API void evolve();

extern "C" EXPLIB3_API void save(int index, int fileID);

extern "C" EXPLIB3_API void load(char* filename);
