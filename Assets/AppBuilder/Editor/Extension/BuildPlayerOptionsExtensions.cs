using System.ComponentModel.DataAnnotations;
using System.Linq;
using UnityEditor;

namespace AppBuilder
{
    public static class BuildPlayerOptionsExtensions
    {
        public static void Validate(this BuildPlayerOptions options)
        {
            if (options.scenes == null || !options.scenes.Any())
            {
                throw new ValidationException("empty build scenes");
            }

            if (string.IsNullOrEmpty(options.locationPathName))
            {
                throw new ValidationException("empty output path");
            }
        }
    }
}