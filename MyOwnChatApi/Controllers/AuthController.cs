using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyOwnChatApi.Domain.DTOs.Auth;
using MyOwnChatApi.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyOwnChatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IRegisterService _registerService;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly ILoginService _loginService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogoutService _logoutService;

        public AuthController(
            ILogger<AuthController> logger,
            IRegisterService registerService,
            IEmailVerificationService emailVerificationService,
            ILoginService loginService,
            IRefreshTokenService refreshTokenService,
            ILogoutService logoutService)
        {
            _logger = logger;
            _registerService = registerService;
            _emailVerificationService = emailVerificationService;
            _loginService = loginService;
            _refreshTokenService = refreshTokenService;
            _logoutService = logoutService;
        }

        // POST: /api/register 仮登録
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequestDto registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "入力の形式が不正です" });
            }

            try
            {
                await _registerService.RegisterAsync(registerRequest);
                return Ok(new { message = "仮登録が完了しました,メールをご確認ください" });
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return BadRequest(new {message=ex.Message});
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return Problem("サーバーエラー");
            }
        }


        // GET: /api/verify 本登録
        [HttpGet("verify")]
        public async Task<ActionResult> VerifyEmail(string token)
        {
            try
            {
                await _emailVerificationService.VerifyEmailAsync(token);

                return Ok(new { message = "本登録が完了しました,ログインしてください" });
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return BadRequest(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return Problem("サーバーエラー");
            }
        }

        // POST: /api/login ログイン
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "不正な形式です" });
            }
            try
            {
                var result = await _loginService.LoginAsync(loginRequest);

                // RefreshTokenをHttpOnly Cookieにセット
                SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiresAt);

                return Ok(new
                {
                    accessToken = result.AccessToken,
                    accessTokenExpiresAt = result.AccessTokenExpiresAt
                });
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return BadRequest(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return Problem("サーバーエラー");
            }
        }

        // POST: /api/refresh
        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new {message="RefreshTokenが存在しません"});
            }

            try
            {
                var result = await _refreshTokenService.RefreshTokenAsync(refreshToken);

                // RefreshTokenをHttpOnly Cookieにセット
                SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiresAt);

                return Ok(new
                {
                    accessToken = result.AccessToken,
                    accessTokenExpiresAt = result.AccessTokenExpiresAt
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return Problem("サーバーエラー");
            }
        }

        // POST: /api/logout ログアウト
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if(userClaim == null)
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません" });
            }

            int userId = int.Parse(userClaim.Value);

            try
            {
                await _logoutService.LogoutAsync(userId);
                Response.Cookies.Delete("refresh_token");
                return Ok(new { message = "ログアウトしました" });
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return BadRequest(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex.StackTrace);
                return Problem("サーバーエラー");
            }
        }


        // Cookie設定
        // Todo: AppServiceにデプロイするときはSecure=trueに変更を必ずする
        private void SetRefreshToken(string refreshToken, DateTimeOffset expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = expires
            };

            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
        }
    }
}
