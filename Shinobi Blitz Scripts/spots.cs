using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class spots : MonoBehaviour {

    public Color hover_color; //the color of the spot once the player hovers the cursor over this building spot.
    public Color insufficient_color; //the color of the spot if the player hovers the cursor over this building spot and doesn't have enough money.
    private Color original_color; //the first color of the node. this is used to return the color back once the player isn't hovering over the node.

    public Vector3 offset; //the offset to properly build the turret ontop of this spot.
    private Renderer rendr; //the renderer to change the color of the building spot.

    public GameObject preview_archer, preview_shuriken, preview_flame, preview_mortar;
    private GameObject previewed_tower;

    public int times_rotated = 0;

    [HideInInspector]
    public GameObject tower; //this is used to keep track of the tower that is built on this spot.
    [HideInInspector]
    public TurretBlueprint turret_blueprint; //the current turret blueprint to remember what kind of turret is built on this spot.
    [HideInInspector]
    public bool is_upgraded = false; //to remember if the turret is upgraded or not.

    buildmanager buildmanager_script; //used to access the build manager script

    public ClickTower click_scr;

    public Vector3 GetBuildPosition() //this will give the build position of where to place the turret.
    {
        //this will give the position of this spot with the additional offset to give the proper building spot.
        return transform.position + offset;
    }

    void BuildTurret(TurretBlueprint _blueprint)
    {
        //check if the player has enough money to build. otherwise nothing happens and an error message happens.
        if (PlayerStats.money < _blueprint.cost || PlayerStats.building_amount >= PlayerStats.building_limit)
        {
            //Debug.Log("Not enough money to build.");
            return;
        }

        //otherwise, remove their money from the cost and build them the tower.
        Destroy(previewed_tower);
        PlayerStats.money -= _blueprint.cost;
        GameObject _tower = (GameObject)Instantiate(_blueprint.prefab, GetBuildPosition(), Quaternion.identity);
        _tower.transform.parent = GameObject.Find("BuildList").transform;
        tower = _tower;

        click_scr = tower.GetComponent<ClickTower>();
        click_scr.pass_spots(this);

        turret_blueprint = _blueprint;
        PlayerStats.building_amount++;
    }

    public void UpgradeTurret()
    {
        //check if the player has enough money to upgrade. otherwise nothing happens and an error message happens.
        if (PlayerStats.money < turret_blueprint.upgrade_cost)
        {
            return;
        }


        //check if the tower is already upgraded.
        if(!is_upgraded)
        {
            //otherwise, remove their old tower and money. then build them the upgraded tower.
            Destroy(tower);
            PlayerStats.money -= turret_blueprint.upgrade_cost;
            GameObject _tower = (GameObject)Instantiate(turret_blueprint.upgraded_prefab, GetBuildPosition(), Quaternion.identity);
            _tower.transform.parent = GameObject.Find("BuildList").transform;
            tower = _tower;

            click_scr = tower.GetComponent<ClickTower>();
            click_scr.pass_spots(this);

            for (int i = 0; i < times_rotated; i++)
            {
                UpgradeRotation();
            }

            is_upgraded = true;
        }
        
        
    }

    public void RotateTurret()
    {   
        //keeps the old x and z rotations while adding 90 to the y.
        tower.transform.eulerAngles = new Vector3(tower.transform.eulerAngles.x, tower.transform.eulerAngles.y + 90, tower.transform.eulerAngles.z);
        times_rotated++;
        if(times_rotated >= 4)
        {
            times_rotated = 0;
        }
    }

    public void UpgradeRotation()
    {
        //keeps the old x and z rotations while adding 90 to the y.
        tower.transform.eulerAngles = new Vector3(tower.transform.eulerAngles.x, tower.transform.eulerAngles.y + 90, tower.transform.eulerAngles.z);
    }

    public void SellTurret()
    {
        //adds money to the player's stats
        PlayerStats.money += turret_blueprint.GetSellAmount();
        //destroy the tower and reset the upgraded boolean to allow for upgrading again
        Destroy(tower);
        is_upgraded = false;
        //sets the blueprint to null because there is no current tower on this spot.
        turret_blueprint = null;

        click_scr = null;

        PlayerStats.building_amount--;
        if (PlayerStats.building_amount < 0)
            PlayerStats.building_amount = 0;
    }

    void OnMouseEnter()
    {
        //blocks the player if they are hovering over a UI overlay.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        //if the player can't build the tower, then nothing happens and exit now.
        if (!buildmanager_script.CanBuild)
            return;

        //if the player has enough money to build, then it will change the color of the spot to grey.
        if(buildmanager_script.HasMoney && tower == null && PlayerStats.building_amount < PlayerStats.building_limit)
        {
            rendr.material.color = hover_color;

            if(shop.archer_selected)
                previewed_tower = Instantiate(preview_archer, GetBuildPosition(), transform.rotation);
            else if (shop.slauncher_selected)
                previewed_tower = Instantiate(preview_shuriken, GetBuildPosition(), transform.rotation);
            else if (shop.flame_selected)
                previewed_tower = Instantiate(preview_flame, GetBuildPosition(), transform.rotation);
            else if (shop.mortar_selected)
                previewed_tower = Instantiate(preview_mortar, GetBuildPosition(), transform.rotation);
        }
        else
        {
            //otherwise they did not have enough money. it will change into red.
            rendr.material.color = insufficient_color;
        }
    }
	

    void OnMouseExit()
    {
        //once the player no longer hovers over the building spot, return to its original color.
        rendr.material.color = original_color;
        Destroy(previewed_tower);
    }

    
    void OnMouseDown()
    {
        //blocks the player if they are hovering over a UI overlay.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        //if there is already a turret, then it will select the node instead.
        if (tower != null) 
        {
            buildmanager_script.SelectNode(this);
            return;
        }

        //if the player aren't able to build, then exit out now.
        if (!buildmanager_script.CanBuild)
            return;

        //otherwise, build the selected turret on top of this node.
        BuildTurret(buildmanager_script.GetTurretToBuild());

    }

    void Start()
    {
        //creates the build manager singleton.
        buildmanager_script = buildmanager.S;
        //gets the renderer of this building spot.
        rendr = GetComponent<Renderer>();
        //save the original color information.
        original_color = rendr.material.color;

    }

    public void Click()
    {
        //blocks the player if they are hovering over a UI overlay.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        //if there is already a turret, then it will select the node instead.
        if (tower != null)
        {
            buildmanager_script.SelectNode(this);
            return;
        }

        //if the player aren't able to build, then exit out now.
        if (!buildmanager_script.CanBuild)
            return;

        //otherwise, build the selected turret on top of this node.
        BuildTurret(buildmanager_script.GetTurretToBuild());
    }

}
