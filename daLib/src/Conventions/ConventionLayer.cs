

namespace daLib.Conventions
{
    public static class ConventionLayer
    {
        public static void Validate(params IConvention[] list)
        {
            foreach (IConvention convention in list)
            {
                if (!convention.isValid())
                {
                    convention.Throw();
                }
            }
        }
    }
}
