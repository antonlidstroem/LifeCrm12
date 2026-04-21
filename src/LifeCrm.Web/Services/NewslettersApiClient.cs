using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class NewslettersApiClient : ApiClientBase
{
    public NewslettersApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }
}
