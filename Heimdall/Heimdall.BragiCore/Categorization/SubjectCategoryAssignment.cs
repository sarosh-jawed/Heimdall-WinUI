using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Extraction;

namespace Heimdall.BragiCore.Categorization;

public sealed record SubjectCategoryAssignment(
    ExtractedSubject Subject,
    CategoryRule CategoryRule,
    string RoutingReason);
