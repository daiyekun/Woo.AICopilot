using System;

namespace Woo.AICopilot.Services.Common.Exceptions;

public class ForbiddenException(string? message) : Exception(message);