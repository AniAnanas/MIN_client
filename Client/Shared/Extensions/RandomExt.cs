using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Shared.Extensions;

public static class RandomExt
{
    public static string NextString(this Random rand, int length)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            switch (rand.Next(0, 3))
            {
                case 0:
                    stringBuilder.Append((char)rand.Next(97, 123));
                    break;
                case 1:
                    stringBuilder.Append((char)rand.Next(65, 91));
                    break;
                case 2:
                    stringBuilder.Append((char)rand.Next(48, 58));
                    break;
            }
        }

        return stringBuilder.ToString();
    }
}
