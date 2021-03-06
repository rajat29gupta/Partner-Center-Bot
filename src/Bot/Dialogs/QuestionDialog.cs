﻿// -----------------------------------------------------------------------
// <copyright file="QuestionDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Dialogs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Providers;

    /// <summary>
    /// Dialog that processes questions from the authenticated user.
    /// </summary>
    [Serializable]
    public class QuestionDialog : IDialog<IMessageActivity>
    {
        /// <summary>
        /// Provides the ability to interact with the question and answer service.
        /// </summary>
        protected readonly IQnAService QnAService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionDialog"/> class.
        /// </summary>
        /// <param name="provider">Provides access to core services.</param>
        public QuestionDialog(IBotProvider provider)
        {
            provider.AssertNotNull(nameof(provider));

            QnAService = new QnAMakerService(new QnAMakerAttribute(
                provider.Configuration.QnASubscriptionKey.ToUnsecureString(),
                provider.Configuration.QnAKnowledgebaseId,
                "default message",
                0.6));
        }

#pragma warning disable 1998
        /// <summary>
        /// The start of the code that represents the conversational dialog.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// </exception>
        public async Task StartAsync(IDialogContext context)
        {
            context.AssertNotNull(nameof(context));

            context.Wait(MessageReceivedAsync);
        }
#pragma warning restore 1998

        /// <summary>
        /// Processes a message in a conversation between the bot and the authenticated user.
        /// </summary>
        /// <param name="context">The context for the execution of a dialogs conversational process.</param>
        /// <param name="argument">The message in a conversation between the bot and a user.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="argument"/> is null.
        /// </exception>
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message;
            QnAMakerResults result = null;

            context.AssertNotNull(nameof(context));
            argument.AssertNotNull(nameof(argument));

            try
            {
                message = await argument;

                if (!string.IsNullOrEmpty(message?.Text))
                {
                    result = await QnAService.QueryServiceAsync(message.Text).ConfigureAwait(false);
                }

                message = context.MakeMessage();
                message.Text = result == null ? Resources.RewordQuestion : result.Answers.First().Answer;

                context.Done(message);
            }
            finally
            {
                message = null;
                result = null;
            }
        }
    }
}