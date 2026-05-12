using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public BottleController FirstBottle;
    public BottleController SecondBottle;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                BottleController clickedBottle = hit.collider.GetComponent<BottleController>();

                if (clickedBottle != null)
                {
                    if (FirstBottle == null)
                    {
                        FirstBottle = clickedBottle;
                    }
                    else
                    {
                        if (FirstBottle == clickedBottle)
                        {
                            FirstBottle = null;
                        }
                        else
                        {
                            SecondBottle = clickedBottle;
                            FirstBottle.bottleControllerRef = SecondBottle;

                            FirstBottle.UpdateTopColorValues();
                            SecondBottle.UpdateTopColorValues();

                            if (SecondBottle.FillBottleCheck(FirstBottle.topColor) == true)
                            {
                                FirstBottle.StartColorTransfer();
                            }

                            FirstBottle = null;
                            SecondBottle = null;
                        }
                    }
                }
            }
        }
    }
}