using UnityEngine;
using UnityEngine.UI;

public class CharacterDeploymentUI : MonoBehaviour
{
    [SerializeField]
    private Button[] characterButtons;

    private DeploymentManager deploymentManager;

    private Character[] characters;

    private void Start()
    {
        deploymentManager =
            FindFirstObjectByType<DeploymentManager>();
    }

    public void SetCharacters(Character[] characters)
    {
        this.characters = characters;

        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;

            characterButtons[i].onClick.RemoveAllListeners();

            characterButtons[i].onClick.AddListener(() =>
            {
                SelectCharacter(index);
            });
        }
    }

    private void SelectCharacter(int index)
    {
        if (characters == null)
            return;

        if (index < 0 || index >= characters.Length)
            return;

        Character character = characters[index];

        if (character == null)
            return;

        deploymentManager.SelectCharacter(character);

        Debug.Log(
            $"Ñ¡Ôñ½ÇÉ«Ãæ°å£º{character.GetCharacterName()}"
        );
    }
}