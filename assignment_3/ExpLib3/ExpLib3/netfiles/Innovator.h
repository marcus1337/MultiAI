#pragma once
#include <map>
#include <utility> 

class Innovator {

    typedef std::pair<int, int> par;
    typedef std::tuple<int,int,int> par3;

    Innovator() : innovNum(0), innovNodeNum(0) {}
    int innovNum, innovNodeNum;

    std::map<par, int> takenIDs;
    std::map<par3, int> takenMiddleIDs;

    template <class T>
    static bool mapContains(std::map<T, int>& amap, T key)
    {
        if (amap.find(key) == amap.end()) return false;
        return true;
    }

public:
    void reset() {
        innovNum = innovNodeNum = 0;
        takenIDs.clear();
        takenMiddleIDs.clear();
    }

    static Innovator& getInstance()
    {
        static Innovator instance; 
        return instance;
    }

    int getNewNodeNum(int from, int to, int children) {
        par3 betweenEdges = std::make_tuple(from, to, children);
        if (mapContains<par3>(takenMiddleIDs, betweenEdges)) {
            return takenMiddleIDs[betweenEdges];
        }
        innovNodeNum++;
        takenMiddleIDs[betweenEdges] = innovNodeNum;
        return innovNodeNum;
    }

    int getAnyNodeNum() {
        innovNodeNum++;
        return innovNodeNum;
    }

    int getMaxNodeNum() {
        return innovNodeNum;
    }
    int getMaxEdgeNum() {
        return innovNum;
    }

    int getNum(int from, int to) {
        par betweenNodes = std::make_pair(from, to);
        if (mapContains<par>(takenIDs, betweenNodes)) {
            return takenIDs[betweenNodes];
        }
        innovNum++;
        takenIDs[betweenNodes] = innovNum;
        return innovNum;
    }

    Innovator(Innovator const&) = delete;
    void operator=(Innovator const&) = delete;

};