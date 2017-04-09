using SC.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SC.UI.Web.Mvc.Controllers.Api
{
    public class TicketResponseController : ApiController
    {
        private readonly ITicketManager mgr = new TicketManager();

        public IHttpActionResult Get(int ticketNumber)
        {
            var responses = mgr.GetTicketResponses(ticketNumber);

            if (responses == null || responses.Count() == 0)
                return StatusCode(HttpStatusCode.NoContent);

            return Ok(responses);
        }
    }
}
