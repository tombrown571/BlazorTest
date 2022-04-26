using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FP.API.Controllers
{
    public class CaseResourceParameters
    {
            private readonly int _maxPageSize;
            private int _pageSize;

            /// <summary>
            /// Default constructor, needed when passed as API parameter
            /// </summary>
            public CaseResourceParameters()
                : this(100, 25)
            { }

            public CaseResourceParameters(int maxPageSize, int defaultPageSize)
            {
                _maxPageSize = maxPageSize;
                _pageSize = defaultPageSize;
            }

            public int PageNumber { get; set; } = 1;

            public int PageSize
            {
                get => _pageSize;
                set => _pageSize = (value > _maxPageSize) ? _maxPageSize : value;
            }

        }
    }
