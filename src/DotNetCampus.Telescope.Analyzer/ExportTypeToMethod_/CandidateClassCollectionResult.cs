namespace DotNetCampus.Telescope.Analyzer;

/// <summary>
/// 候选收集的结果。
/// 包含所有的导出结果，例如项目里面有多个导出方法，每一个导出方法对应一个 <see cref="CandidateClassTypeResult"/>。
/// </summary>
internal class CandidateClassCollectionResult(IReadOnlyList<CandidateClassTypeResult> candidateClassTypeResultList)
{
    public IReadOnlyList<CandidateClassTypeResult> CandidateClassTypeResultList { get; } = candidateClassTypeResultList;
}
