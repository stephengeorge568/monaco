using Microsoft.AspNetCore.Mvc;
using Monaco.Services.Interfaces;
using server.models;

namespace Server.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly IDocumentService _docService;

    public TestController(IDocumentService documentService)
    {
        _docService = documentService;
    }

    [HttpGet]
    public State Get()
    {
        return new State {
            Model = _docService.Model,
            RevisionId = _docService.RevisionId,
            History = _docService.History,
            PreHistory = _docService.PreHistory,
        };
    }

    [HttpDelete]
    public void Reset()
    {
        _docService.Reset();
    }

    public class State
    {
        public string Model { get; set; }
        public int RevisionId { get; set; }
        public Dictionary<int, List<Operation>> History { get; set; }
        public Dictionary<int, List<Operation>> PreHistory { get; set; }
    }
}
