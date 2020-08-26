using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using PeiuPlatform.Model.Database;
using PeiuPlatform.Model.ExchangeModel;
using PeiuPlatform.Model.IdentityModel;
using PeiuPlatform.Models.Mysql;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PeiuPlatform.App.App.Controllers
{
    [Produces("application/json")]
    //[Authorize]
    //[EnableCors(origins: "http://www.peiu.co.kr:3011", headers: "*", methods: "*")]
    [ApiController]
    [Route("api/[controller]")]
    //[RequireHttps]
    public class BoardController : ControllerBase
    {
        private readonly UserManager<UserAccountEF> _userManager;
        MysqlDataContext _accountContext;
        private readonly SignInManager<UserAccountEF> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<Role> roleManager;
        private readonly IHTMLGenerator htmlGenerator;
        private readonly ILogger<BoardController> logger;
        private readonly IClaimServiceFactory _claimsManager;
        private readonly AccountEF _accountEF;
        private readonly string WebServerUrl;
        public BoardController(UserManager<UserAccountEF> userManager,
            SignInManager<UserAccountEF> signInManager, RoleManager<Role> _roleManager,
            IEmailSender emailSender, IHTMLGenerator _htmlGenerator, IClaimServiceFactory claimsManager, ILogger<BoardController> logger,
            IConfiguration config, AccountEF accountEF, MysqlDataContext accountDataContext)
        {
            _userManager = userManager;
            _accountContext = accountDataContext;
            _signInManager = signInManager;
            _emailSender = emailSender;
            htmlGenerator = _htmlGenerator;
            roleManager = _roleManager;
            _claimsManager = claimsManager;
            _accountEF = accountEF;
            this.logger = logger;
        }

        [HttpGet, Route("readboard")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadBoard(int BoardType, int BoardId)
        {
            using (var session = _accountContext.SessionFactory.OpenSession())
            using (var Trans = session.BeginTransaction())
            {
                AttachableViewBoardModel model = new AttachableViewBoardModel();
                model.BoardType = BoardType;
                model.Username = HttpContext.User.Identity.Name;
                var attachFiles = await session.CreateCriteria<AttachFileRepositary>()
                .Add(Restrictions.Eq("Boardid", BoardId) && Restrictions.Eq("Grouptype", BoardType)).ListAsync<AttachFileRepositary>();

                List<ViewFileModel> attachViewFiles = new List<ViewFileModel>();
                foreach (AttachFileRepositary file in attachFiles)
                {
                    ViewFileModel fileModel = new ViewFileModel() { FileId = file.Id, Filename = file.Filename, Filetype = file.Contentstype, KBSize = file.Contents.Length / 1024 };
                    attachViewFiles.Add(fileModel);
                }

                switch (BoardType)
                {
                    case 0: // 공지사항
                        NoticeBoard board = await session.CreateCriteria<NoticeBoard>().Add(Restrictions.Eq("ID", BoardId)).UniqueResultAsync<NoticeBoard>();
                        if (board == null)
                            return BadRequest("존재하지 않는 게시글입니다");
                        //UserAccount account = await session.CreateCriteria<UserAccount>().Add(Restrictions.Eq("ID", BoardId)).UniqueResultAsync<UserAccount>();
                        model.Title = board.Title;
                        model.Contents = board.Contents;
                        board.ViewCount++;
                        model.Createts = board.Createts;
                        model.ViewCount = board.ViewCount;
                        model.VIewFileModels = attachViewFiles.ToArray();
                        await session.UpdateAsync(board);
                        await Trans.CommitAsync();
                        break;
                    default:
                        return BadRequest("없는 게시판의 글입니다");
                }
                return Ok(model);

            }
        }

        [HttpGet, Route("downloadattachfile")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadAttachFile(string fileid)
        {
            using (var session = _accountContext.SessionFactory.OpenSession())
            using (var Trans = session.BeginTransaction())
            {
                var attachFiles = await session.CreateCriteria<AttachFileRepositary>()
                .Add(Restrictions.Eq("Id", fileid)).UniqueResultAsync<AttachFileRepositary>();
                if (attachFiles != null)
                {
                    attachFiles.Downloadcount++;
                    await session.UpdateAsync(attachFiles);
                    await Trans.CommitAsync();
                    // Response...
                    System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = Uri.EscapeUriString(attachFiles.Filename),
                        Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
                    };
                    Response.Headers.Add("Content-Disposition", cd.ToString());
                    Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    return File(attachFiles.Contents, "application/octet-stream");
                }
                else
                    return BadRequest();

            }

        }


        [HttpPut, Route("writeboard")]
        [Authorize(Policy = UserPolicyTypes.OnlySupervisor)]
        public async Task<IActionResult> WriteBoard([FromBody] AttachableBoardModel boardModel)
        {

            using (var session = _accountContext.SessionFactory.OpenSession())
            using (var trans = session.BeginTransaction())
            {
                NoticeBoard NewBoard = new NoticeBoard();
                NewBoard.RegisterUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                NewBoard.Title = boardModel.Title;
                NewBoard.Contents = boardModel.Contents;
                NewBoard.Createts = DateTime.Now;

                await session.SaveOrUpdateAsync(NewBoard);

                if (boardModel.AttachFiles.Length > 0)
                {
                    foreach (AttachFileModel attach in boardModel.AttachFiles)
                    {
                        AttachFileRepositary attachFile = RegisterFile(0, NewBoard.ID, attach.Filename, attach.ContentsBase64);
                        await session.SaveOrUpdateAsync(attachFile);
                    }
                }
                await trans.CommitAsync();
            }
            return Ok();
        }

        //[HttpPut()]
        //[Authorize(Policy = UserPolicyTypes.OnlySupervisor)]
        //public void ModifyBoard(int typeid, int id, [FromBody] BoardModel boardModel)
        //{

        //}

        //[HttpPut()]
        //[Authorize(Policy = UserPolicyTypes.OnlySupervisor)]
        //public void RemoveBoard(int typeid, int id)
        //{

        //}


        private AttachFileRepositary RegisterFile(int boardtype, int boardid, string fileName, string FileBase64Data)
        {
            if (string.IsNullOrEmpty(FileBase64Data))
                return null;
            AttachFileRepositary registerFile = new AttachFileRepositary();
            registerFile.Id = Guid.NewGuid().ToString();
            registerFile.Filename = fileName;
            registerFile.Boardid = boardid;
            registerFile.Grouptype = boardtype;
            string[] splits = FileBase64Data.Split(',');
            string contentsType = "";
            string contents = "";
            if (splits.Length == 1)
            {
                contentsType = System.IO.Path.GetExtension(fileName);
                contents = splits[0];
            }
            else

            {
                contentsType = splits[0];
                contents = splits[1];
            }
            registerFile.Contentstype = contentsType;
            byte[] data = Convert.FromBase64String(contents);
            registerFile.Contents = data;
            registerFile.Createdt = DateTime.Now.Date;
            return registerFile;
        }
    }
}
