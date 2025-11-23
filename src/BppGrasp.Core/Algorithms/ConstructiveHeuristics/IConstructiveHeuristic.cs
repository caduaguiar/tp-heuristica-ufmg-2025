using BppGrasp.Core.Models;

namespace BppGrasp.Core.Algorithms.ConstructiveHeuristics;

public interface IConstructiveHeuristic
{
    Solution Construct(BppInstance instance, double alpha, Random random);
}
