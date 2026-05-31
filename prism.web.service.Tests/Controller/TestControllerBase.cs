using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace prism.web.service.Tests.Controller
{
    [TestClass]
    public class TestControllerBase<T> where T : class, new()
    {
        protected T _controller;

        [TestInitialize]
        public void Setup()
        {
            _controller = new T();
        }
    }
}
