namespace BezierSolution
{
	public interface IBezierWalker
	{
		BezierSplineAdvanced Spline { get; }
		float NormalizedT { get; }
		bool MovingForward { get; }
	}
}