using System;
using System.Linq;
using UnityEngine;
using BullyingGame.LevelOne;

namespace BullyingGame.Campaign
{
    [RequireComponent(typeof(LevelOneView))]
    public sealed class CampaignChallenges : MonoBehaviour
    {
        LevelOneView view;
        CampaignStep step;
        CampaignChapterProgress progress;
        Action accepted, persist, close;
        string feedback;

        public void Open(CampaignStep task, CampaignChapterProgress checkpoint, Action completed, Action save, Action back)
        {
            view = GetComponent<LevelOneView>();
            step = task;
            progress = checkpoint;
            accepted = completed;
            persist = save;
            close = back;
            feedback = "";
            if (task.kind == CampaignTaskKind.Order) DrawOrder();
            else DrawQuestion();
        }

        void DrawOrder()
        {
            if (progress.orderBank.Length != step.order.Length)
            {
                progress.orderBank = step.order.OrderBy(_ => UnityEngine.Random.value).ToArray();
                persist();
            }
            var available = progress.orderBank.Except(progress.selectedOrder).ToArray();
            if (available.Length == 0) { accepted(); return; }
            string body = $"{step.objective}\n{progress.selectedOrder.Length} / {step.order.Length} tersusun\n" +
                string.Join(" → ", progress.selectedOrder) + "\n" + feedback;
            view.ShowOptions(step.title, body, available, index =>
            {
                string choice = available[index];
                if (step.order[progress.selectedOrder.Length] != choice)
                {
                    progress.failures++;
                    feedback = "Periksa urutan pada petunjuk. Bagian yang sudah benar tetap tersimpan.";
                    if (progress.failures >= 2) feedback += "\nBerikutnya: " + step.order[progress.selectedOrder.Length];
                }
                else
                {
                    progress.selectedOrder = progress.selectedOrder.Concat(new[] { choice }).ToArray();
                    feedback = "Benar. Lanjutkan bagian berikutnya.";
                }
                persist();
                DrawOrder();
            }, close);
        }

        void DrawQuestion()
        {
            bool choosing = step.kind == CampaignTaskKind.Choice;
            if (!choosing && progress.question >= step.questions.Length) { accepted(); return; }
            var question = step.questions[choosing ? 0 : progress.question];
            string heading = step.questions.Length > 1 ? $"{step.title}   ·   {progress.question + 1}/{step.questions.Length}" : step.title;
            view.ShowOptions(heading, question.text + "\n\n" + feedback, question.options, index =>
            {
                if (step.kind == CampaignTaskKind.Choice)
                {
                    progress.question = index;
                    persist();
                    accepted();
                    return;
                }
                if (index != question.answer)
                {
                    progress.failures++;
                    feedback = question.feedback;
                }
                else
                {
                    progress.question++;
                    feedback = "";
                }
                persist();
                DrawQuestion();
            }, close);
        }
    }
}
