using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace CcshlRateSheet
{
    public class AutoMapperConfig
    {
        public static void MapModels()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<RateHistory, Rate>();
                cfg.CreateMap<Rate, RateHistory>();
            });
        }

    }
}
