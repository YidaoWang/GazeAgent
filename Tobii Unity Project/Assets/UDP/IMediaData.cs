﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.UDP
{
    public interface IMediaData: IDisposable
    {
        MediaCondition MediaCondition { get; }
        byte[] ToBytes();
    }
}
