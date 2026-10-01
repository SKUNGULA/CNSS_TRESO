using CNSS_TRESO.Application.Security;

namespace CNSS_TRESO.Application.Security;

public interface ICallerContextAccessor
{
    CallerContext? Current { get; }
}
